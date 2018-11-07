// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using k8s;
using k8s.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Oryx.Integration.Tests
{
    public class SampleAppsTests : IClassFixture<SampleAppsTests.SampleAppsFixture>
    {
        private readonly ITestOutputHelper _output;

        private static readonly string NAMESPACE = "default";
        private static readonly string STORAGE_KEY_VAR = "STORAGEACCOUNTKEY";
        private static readonly string BUILD_NUMBER_VAR = "BUILD_BUILDNUMBER";
        private static readonly string KUBECONFIG_VAR = "KUBECONFIG";
        private static readonly string SHARE_NAME = "oryx";
        private static readonly int MAX_RESTARTS = 4;

        private SampleAppsFixture fixture;

        public SampleAppsTests(ITestOutputHelper output, SampleAppsTests.SampleAppsFixture fixture)
        {
            _output = output;
            this.fixture = fixture;
        }

        public class SampleAppsFixture : IDisposable
        {
            private V1PersistentVolume storage;
            private V1PersistentVolumeClaim storageClaim;

            public SampleAppsFixture()
            {
                BuildNumber = Environment.GetEnvironmentVariable(BUILD_NUMBER_VAR) ?? Guid.NewGuid().ToString();

                var storageKey = Environment.GetEnvironmentVariable(STORAGE_KEY_VAR);
                Console.WriteLine("Using storage key \"{0}...\" from environment variable \"{1}\"", storageKey.Substring(0, 4), STORAGE_KEY_VAR);
      
                FolderName = "SampleApps" + BuildNumber;

                UploadToFileshare(storageKey, SHARE_NAME, FolderName);

                string kubeConfig = Environment.GetEnvironmentVariable(KUBECONFIG_VAR);
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
                
                // Create a PV for our Azure File share and a corresponding claim
                // If these fail, make sure that they don't already exist in the cluster: `kubectl delete pvc --all; kubectl delete pv --all`
                storage = Client.CreatePersistentVolume(LoadYamlConfig<V1PersistentVolume>("build-volume.yaml", SHARE_NAME));
                storageClaim = Client.CreateNamespacedPersistentVolumeClaim(LoadYamlConfig<V1PersistentVolumeClaim>("build-volume-claim.yaml"), NAMESPACE);
                Console.WriteLine("Created PersistentVolume and correspoinding PersistentVolumeClaim");

                // Create the build pod
                V1Pod podSpec = LoadYamlConfig<V1Pod>("build-image.yaml", "azure-build-image" + BuildNumber, storageClaim.Metadata.Name);
                BuildPod = Client.CreateNamespacedPod(podSpec, NAMESPACE);
                Console.WriteLine("Created build pod");
                // Wait for the pod to start
                while ("Pending".Equals(BuildPod.Status.Phase))
                {
                    Console.WriteLine("Waiting 10s for build pod to start");
                    Thread.Sleep(10000);
                    BuildPod = Client.ReadNamespacedPodStatus(BuildPod.Metadata.Name, NAMESPACE);
                }
            }

            public void Dispose()
            {
                if (BuildPod != null)
                {
                    Client.DeleteNamespacedPod(new V1DeleteOptions(), BuildPod.Metadata.Name, NAMESPACE);
                    Console.WriteLine("Deleted build pod");
                }
                if (storageClaim != null)
                {
                    Client.DeleteNamespacedPersistentVolumeClaim(new V1DeleteOptions(), storageClaim.Metadata.Name, NAMESPACE);
                    Console.WriteLine("Deleted build PersistentVolumeClaim");
                }
                if (storage != null)
                {
                    Client.DeletePersistentVolume(new V1DeleteOptions(), storage.Metadata.Name);
                    Console.WriteLine("Deleted build PersistentVolume");
                }
            }

            public V1Pod BuildPod { get; }

            public string BuildNumber { get; }

            public string FolderName { get; }

            public IKubernetes Client { get; }

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
        }

        [Theory]
        [InlineData("webfrontend", "oryxdevms/node-8.1:latest", "nodejs", "8.1")]
//        [InlineData("flask-app", "oryxdevms/python-3.7:latest", "python", "3.7")]
        [InlineData("linxnodeexpress", "oryxdevms/node-4.4:latest", "nodejs", "4.4.7")]
        public void CanBuildAndRunSampleApp(string appName, string runtimeImage, string language, string languageVersion)
        {
            V1Deployment runtimeDeploymentSpec = null;
            V1Service runtimeServiceSpec = null;

            try
            {
                Assert.Equal("Running", fixture.BuildPod.Status.Phase);
                Console.WriteLine("Build pod is running");
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
                
                Console.WriteLine("Running command in build pod: `{0}`", string.Join(' ', command));
                ExecInPodAsync(fixture.Client, fixture.BuildPod, command).GetAwaiter().GetResult();
                // Wait for build to finish. Todo: figure out when exactly build process completes
                Console.WriteLine("Waiting 15s for build to finish");
                Thread.Sleep(15000);

                // create a deployment with runtime image and run the compiled app
                runtimeDeploymentSpec = LoadYamlConfig<V1Deployment>("runtime-deployment.yaml", appName, fixture.BuildNumber, runtimeImage, language);
                var runtimeDeployment = fixture.Client.CreateNamespacedDeployment(runtimeDeploymentSpec, NAMESPACE);
                Console.WriteLine("Waiting 10s for runtime deployment");
                Thread.Sleep(10000);
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

                // Wait till pod is running and container created and started
                while ("Pending".Equals(runtimePod.Status.Phase) || runtimePod.Status.ContainerStatuses.Count == 0 ||
                    !runtimePod.Status.ContainerStatuses.First().Ready && runtimePod.Status.ContainerStatuses.First().RestartCount < MAX_RESTARTS)
                {
                    Console.WriteLine("Waiting 10s for runtime pod to start");
                    Thread.Sleep(10000);
                    runtimePod = fixture.Client.ReadNamespacedPodStatus(runtimePod.Metadata.Name, NAMESPACE);
                }
                string message = "";
                if (runtimePod.Status.ContainerStatuses.First().State.Waiting != null)
                {
                    message += runtimePod.Status.ContainerStatuses.First().State.Waiting.Message;
                }
                Assert.True(runtimePod.Status.ContainerStatuses.First().Ready, "Runtime container failed to start: " + message);

                // Create load balancer for the deployed app
                runtimeServiceSpec = LoadYamlConfig<V1Service>("runtime-service.yaml", appName + fixture.BuildNumber);

                var runtimeService = fixture.Client.CreateNamespacedService(runtimeServiceSpec, NAMESPACE);
                while (runtimeService.Status.LoadBalancer.Ingress == null || runtimeService.Status.LoadBalancer.Ingress.Count == 0)
                {
                    Console.WriteLine("Waiting 10s for load balancer service to start");
                    Thread.Sleep(10000);
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

        private static T LoadYamlConfig<T>(string configName, params object[] formatArgs)
        {
            string yaml = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "configurations", configName));
            if (formatArgs.Length > 0)
            {
                yaml = string.Format(yaml, formatArgs);
            }
            return Yaml.LoadFromString<T>(yaml);
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
