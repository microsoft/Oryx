// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using k8s;
using k8s.Models;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;
using Xunit;
using Xunit.Abstractions;

namespace Oryx.Integration.Tests
{
    public class SampleAppsTests : IClassFixture<SampleAppsTests.SampleAppsFixture>
    {
        private readonly ITestOutputHelper _output;

        private static readonly string NAMESPACE = "default";
        private static readonly string SHARE_NAME = "oryx";

        private SampleAppsFixture fixture;

        public SampleAppsTests(ITestOutputHelper output, SampleAppsTests.SampleAppsFixture fixture)
        {
            _output = output;
            this.fixture = fixture;
        }

        public class SampleAppsFixture : IDisposable
        {
            private V1Pod podSpec;

            public SampleAppsFixture()
            {
                BuildNumber = Environment.GetEnvironmentVariable("BUILD_BUILDNUMBER") == null ?
                    Guid.NewGuid().ToString() : Environment.GetEnvironmentVariable("BUILD_BUILDNUMBER");

                var storageKey = Environment.GetEnvironmentVariable("STORAGEACCOUNTKEY");
      
                FolderName = "SampleApps" + BuildNumber;

                UploadToFileshare(storageKey, SHARE_NAME, FolderName);

                string kubeConfig = Environment.GetEnvironmentVariable("KUBECONFIG");
                KubernetesClientConfiguration config;
                if (string.IsNullOrEmpty(kubeConfig))
                {
                    config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
                }
                else
                {
                    MemoryStream stream = new MemoryStream();
                    StreamWriter writer = new StreamWriter(stream);
                    writer.Write(kubeConfig);
                    writer.Flush();
       
                    config = KubernetesClientConfiguration.BuildConfigFromConfigFile(stream);
                }
                Client = new Kubernetes(config);
                var yamlBuildImageSpec = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "configurations", "build-image.yaml"));
                podSpec = Yaml.LoadFromString<V1Pod>(string.Format(yamlBuildImageSpec, "azure-build-image" + BuildNumber));
                BuildPod = Client.CreateNamespacedPod(podSpec, NAMESPACE);
                // wait for the pod to start
                while ("Pending".Equals(BuildPod.Status.Phase))
                {
                    Task.Delay(10000).GetAwaiter().GetResult();
                    BuildPod = Client.ReadNamespacedPodStatus(podSpec.Metadata.Name, NAMESPACE);
                }
            }

            public void Dispose()
            {
                if (podSpec != null)
                {
                    Client.DeleteNamespacedPod(new V1DeleteOptions(), podSpec.Metadata.Name, NAMESPACE);
                }
            }

            public V1Pod BuildPod { get; }

            public string BuildNumber { get; }

            public string FolderName { get; }

            public IKubernetes Client { get; }
        }

        [Theory]
        [InlineData("webfrontend", "oryxdevms/node-8.1:latest", "nodejs", "8.1")]
//        [InlineData("flask-app", "oryxdevms/python-3.7.0:latest", "python", "3.7")]
        [InlineData("linxnodeexpress", "oryxdevms/node-4.4:latest", "nodejs", "4.4.7")]
        public void CanBuildAndRunSampleApp(string appName, string runtimeImage, string language, string languageVersion)
        {
            V1Deployment runtimeDeploymentSpec = null;
            V1Service runtimeServiceSpec = null;
            V1Pod p = null;
            try
            {
                Assert.Equal("Running", fixture.BuildPod.Status.Phase);
                var appFolder = string.Format("/mnt/samples/{0}/{1}/{2}", fixture.FolderName, language, appName);
                
                // execute build command on a pod with build image
                var command = new[]
                {
                    "oryx",
                    "build",
                    appFolder,
                    "-o",
                    appFolder + "_out",
                    "-l",
                    language,
                    "--language-version",
                    languageVersion
                };

                ExecInPodAsync(fixture.Client, fixture.BuildPod, command).GetAwaiter().GetResult();
                // Wait for build to finish. Todo: figure out when exactly build process completes
                Task.Delay(15000).GetAwaiter().GetResult();

                // create a deployment with runtime image and run the compiled app
                var yamlRuntimeDeploymentSpec = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "configurations", "runtime-deployment.yaml"));
                runtimeDeploymentSpec = Yaml.LoadFromString<V1Deployment>(string.Format(yamlRuntimeDeploymentSpec, appName, fixture.BuildNumber, runtimeImage, language));
                var runtimeDeployment = fixture.Client.CreateNamespacedDeployment(runtimeDeploymentSpec, NAMESPACE);
                Task.Delay(10000).GetAwaiter().GetResult();
                // find the pod that was created for deployment
                string podPrefix = appName + fixture.BuildNumber;
                V1PodList podList = fixture.Client.ListNamespacedPod(NAMESPACE);
                V1Pod runtimePod = null; 
                foreach (V1Pod curPod in podList.Items)
                {
                    if (curPod.Metadata.Name.StartsWith(podPrefix))
                    {
                        runtimePod = curPod;
                        break;
                    }            
                }
                // Check if runtime pod was created
                Assert.NotNull(runtimePod);
                
                // Wait till pod is running and container created and started
                while ("Pending".Equals(runtimePod.Status.Phase) || runtimePod.Status.ContainerStatuses.Count == 0 || 
                    !runtimePod.Status.ContainerStatuses.First().Ready && runtimePod.Status.ContainerStatuses.First().RestartCount < 4)
                {
                    Task.Delay(10000).GetAwaiter().GetResult();
                    runtimePod = fixture.Client.ReadNamespacedPodStatus(runtimePod.Metadata.Name, NAMESPACE);
                }
                string message = "";
                if (runtimePod.Status.ContainerStatuses.First().State.Waiting != null)
                {
                    message += runtimePod.Status.ContainerStatuses.First().State.Waiting.Message;
                }
                Assert.True(runtimePod.Status.ContainerStatuses.First().Ready, "Runtime container failed to start: " + message);

                // Create load balancer for the deployed app
                var yamlRuntimeServiceSpec = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "configurations","runtime-service.yaml"));
                runtimeServiceSpec = Yaml.LoadFromString<V1Service>(string.Format(yamlRuntimeServiceSpec, appName + fixture.BuildNumber));

                var runtimeService = fixture.Client.CreateNamespacedService(runtimeServiceSpec, NAMESPACE);
                while (runtimeService.Status.LoadBalancer.Ingress == null || runtimeService.Status.LoadBalancer.Ingress.Count == 0)
                {
                    Task.Delay(10000).GetAwaiter().GetResult();
                    runtimeService = fixture.Client.ReadNamespacedServiceStatus(runtimeServiceSpec.Metadata.Name, NAMESPACE);
                }

                // Ping the app to verify its availability
                var appIp = runtimeService.Status.LoadBalancer.Ingress.First().Ip;
                _output.WriteLine(string.Format("Your app {0} is available at {1}", appName, runtimeService.Status.LoadBalancer.Ingress.First().Ip));
                var response = CheckAddress("http://" + appIp);
                Assert.True(response.IsSuccessStatusCode);
                _output.WriteLine(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            }
            catch (Exception ex)
            {
                _output.WriteLine(ex.Message);
                throw ex;
            }
            finally 
            {
                if (fixture.Client != null)
                {
                    if (runtimeDeploymentSpec != null)
                    {
                        var status = fixture.Client.DeleteNamespacedDeployment(new V1DeleteOptions(),
                            runtimeDeploymentSpec.Metadata.Name, NAMESPACE);
                    }
                    if (runtimeServiceSpec != null)
                    {
                        fixture.Client.DeleteNamespacedService(new V1DeleteOptions(), runtimeServiceSpec.Metadata.Name,
                            NAMESPACE);
                    }
                }
            }
        }

        private static void UploadToFileshare(string storageKey, string shareName, string folderName)
        {
            CloudFileShare fileShare = CloudStorageAccount.Parse(
                    $"DefaultEndpointsProtocol=https;AccountName=oryxautomation;AccountKey={storageKey};EndpointSuffix=core.windows.net")
                .CreateCloudFileClient()
                .GetShareReference(shareName);

            var rootDir = fileShare.GetRootDirectoryReference();
            var sampleAppsFolder = rootDir.GetDirectoryReference(folderName);
            sampleAppsFolder.CreateIfNotExistsAsync().Wait();

            var hostSamplesDir = Path.Combine(Directory.GetCurrentDirectory(), "SampleApps");
            UploadFolder(hostSamplesDir, sampleAppsFolder);
        }

        private static void UploadFolder(string path, CloudFileDirectory sampleAppsFolder)
        {
            string[] files;
            string[] directories;

            files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                //Create a reference to the filename that you will be uploading
                CloudFile cloudFile = sampleAppsFolder.GetFileReference(new FileInfo(file).Name);

                Stream fileStream = File.OpenRead(file);
                cloudFile.UploadFromStreamAsync(fileStream).Wait();
                fileStream.Dispose();
            }

            directories = Directory.GetDirectories(path);
            foreach (string directory in directories)
            {
                var subFolder = sampleAppsFolder.GetDirectoryReference(new DirectoryInfo(directory).Name);
                subFolder.CreateIfNotExistsAsync().Wait();
                UploadFolder(directory, subFolder);
            }
        }

        private static async Task ExecInPodAsync(IKubernetes client, V1Pod pod, string[] commands)
        {
            var webSocket = await client.WebSocketNamespacedPodExecAsync(pod.Metadata.Name, NAMESPACE, commands, pod.Spec.Containers[0].Name);

            var demux = new StreamDemuxer(webSocket);
            demux.Start();

            var buff = new byte[4096];
            var stream = demux.GetStream(1, 1);
            var read = stream.Read(buff, 0, 4096);
            var str = System.Text.Encoding.Default.GetString(buff);
            
            Console.WriteLine(str);
        }

        public static HttpResponseMessage CheckAddress(string url)
        {
            var client = new HttpClient {Timeout = TimeSpan.FromSeconds(600)};
            return client.GetAsync(url).Result;
        }
    }
}
