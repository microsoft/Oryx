// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using k8s;
using k8s.Models;
using Microsoft.Rest;
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

namespace Oryx.Integration.Tests.AksTests
{
    public class SampleAppsTests : IClassFixture<SampleAppsTests.SampleAppsFixture>
    {
        private readonly ITestOutputHelper _output;

        private static readonly string STORAGE_KEY_VAR = "STORAGEACCOUNTKEY";
        private static readonly string BUILD_NUMBER_VAR = "BUILD_BUILDNUMBER";
        private static readonly string KUBECONFIG_VAR = "KUBECONFIG";

        private static readonly string NAMESPACE = "default";
        private static readonly string BUILD_POD_NAME = "builder";
        private static readonly string VOLUME_NAME = "samples";
        private static readonly string STORAGE_ACCOUNT_NAME = "oryxautomation";
        private static readonly string STORAGE_SHARE_NAME = "oryx";
        private static readonly string STORAGE_REQUESTED_CAPACITY = "1Gi";

        private SampleAppsFixture fixture;

        private static HttpClient httpClient = new HttpClient();

        public SampleAppsTests(ITestOutputHelper output, SampleAppsFixture fixture)
        {
            _output = output;
            this.fixture = fixture;
        }

        public class SampleAppsFixture : IDisposable
        {
            private CloudFileShare fileShare;
            private V1PersistentVolume storage;
            private V1PersistentVolumeClaim storageClaim;

            public SampleAppsFixture()
            {
                BuildNumber = Environment.GetEnvironmentVariable(BUILD_NUMBER_VAR) ?? Guid.NewGuid().ToString();
                BuildNumber = BuildNumber.Replace('.', '_'); // Dots are invalid in Kubernetes service names

                var storageKey = Environment.GetEnvironmentVariable(STORAGE_KEY_VAR);
                Console.WriteLine("Using storage key \"{0}...\" from environment variable \"{1}\"", storageKey.Substring(0, 4), STORAGE_KEY_VAR);

                FolderName = "SampleApps-" + BuildNumber;

                fileShare = CloudStorageAccount.Parse($"DefaultEndpointsProtocol=https;AccountName={STORAGE_ACCOUNT_NAME};AccountKey={storageKey};EndpointSuffix=core.windows.net")
                    .CreateCloudFileClient()
                    .GetShareReference(STORAGE_SHARE_NAME);

                UploadToFileShare(fileShare, FolderName);

                KubernetesClientConfiguration config;
                string kubeConfig = Environment.GetEnvironmentVariable(KUBECONFIG_VAR);
                if (string.IsNullOrEmpty(kubeConfig))
                {
                    config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
                }
                else
                {
                    byte[] configByteArray = System.Text.Encoding.UTF8.GetBytes(kubeConfig);
                    using (var stream = new MemoryStream(configByteArray))
                    {
                        config = KubernetesClientConfiguration.BuildConfigFromConfigFile(stream);
                    }
                }
                Client = new Kubernetes(config);

                // Create a PV for our Azure File share and a corresponding claim if they don't already exist
                // If these fail, make sure that they don't already exist in the cluster: `kubectl delete -n default pvc,pv --all`
                storage = Client.CreatePersistentVolume(Specs.BuildVolume.GetSpec(VOLUME_NAME, STORAGE_REQUESTED_CAPACITY, STORAGE_SHARE_NAME));
                storageClaim = Client.CreateNamespacedPersistentVolumeClaim(Specs.BuildVolumeClaim.GetSpec(STORAGE_REQUESTED_CAPACITY), NAMESPACE);
                Console.WriteLine("Created PersistentVolume and corresponding PersistentVolumeClaim");

                // Create the build pod
                var podSpec = Specs.BuildPod.GetSpec(BUILD_POD_NAME, VOLUME_NAME, storageClaim.Metadata.Name);
                BuildPod = k8sHelpers.CreatePodAndWait(Client, podSpec, NAMESPACE, k8sHelpers.IsPodRunning).Result;
                Console.WriteLine("Build pod is up & running");
            }

            public void Dispose()
            {
                Client.DeleteNamespacedPod(new V1DeleteOptions(), BUILD_POD_NAME, NAMESPACE);
                Client.DeleteNamespacedPersistentVolumeClaim(new V1DeleteOptions(), storageClaim.Metadata.Name, NAMESPACE);
                Client.DeletePersistentVolume(new V1DeleteOptions(), VOLUME_NAME);
                Console.WriteLine("Deleted build pod, volume & volume claim");
            }

            public V1Pod BuildPod { get; }

            public string BuildNumber { get; }

            public string FolderName { get; }

            public IKubernetes Client { get; }

            private async static void UploadFolderAsync(string path, CloudFileDirectory sampleAppsFolder)
            {
                string[] files;
                string[] directories;

                files = Directory.GetFiles(path);
                foreach (string file in files)
                {
                    //Create a reference to the filename that you will be uploading
                    CloudFile cloudFile = sampleAppsFolder.GetFileReference(new FileInfo(file).Name);

                    using (Stream fileStream = File.OpenRead(file))
                    {
                        await cloudFile.UploadFromStreamAsync(fileStream);
                    }
                }

                directories = Directory.GetDirectories(path);
                foreach (string directory in directories)
                {
                    var subFolder = sampleAppsFolder.GetDirectoryReference(new DirectoryInfo(directory).Name);
                    await subFolder.CreateIfNotExistsAsync();
                    UploadFolderAsync(directory, subFolder);
                }
            }

            private async static void UploadToFileShare(CloudFileShare fileShare, string folderName)
            {
                var rootDir = fileShare.GetRootDirectoryReference();
                var sampleAppsFolder = rootDir.GetDirectoryReference(folderName);
                await sampleAppsFolder.CreateIfNotExistsAsync();

                var hostSamplesDir = Path.Combine(Directory.GetCurrentDirectory(), "SampleApps");
                UploadFolderAsync(hostSamplesDir, sampleAppsFolder);
            }
        }

        [Theory, Trait("E2E", "AKS")]
        [InlineData("linxnodeexpress", "oryxdevms/node-4.4:latest", "nodejs", "4.4.7")]
        [InlineData("webfrontend", "oryxdevms/node-8.1:latest", "nodejs", "8.1")]
        //[InlineData("flask-app", "oryxdevms/python-3.7.0:latest", "python", "3.7")]
        public async Task CanBuildAndRunSampleApp(string appName, string runtimeImage, string language, string languageVersion)
        {
            V1Deployment runtimeDeployment = null;
            V1Service runtimeService = null;

            try
            {
                var appFolder = string.Format("/mnt/samples/{0}/{1}/{2}", fixture.FolderName, language, appName);

                // execute build command on a pod with build image
                var command = new[]
                {
                    "oryx", "build", appFolder,
                    "-o", appFolder + "_out",
                    "-l", language,
                    "--language-version", languageVersion
                };

                Console.WriteLine("Running command in build pod: `{0}`", string.Join(' ', command));
                string buildOutput = await k8sHelpers.ExecInPodAsync(fixture.Client, fixture.BuildPod, NAMESPACE, command);
                Console.WriteLine("> " + buildOutput.Replace("\n", "\n> ") + Environment.NewLine);

                // Create a deployment with runtime image and run the compiled app
                var runtimeDeploymentSpec = Specs.RunTimeDeployment.GetSpec(appName + fixture.BuildNumber, appName, runtimeImage, appFolder, VOLUME_NAME, STORAGE_SHARE_NAME);
                runtimeDeployment = await k8sHelpers.CreateDeploymentAndWait(fixture.Client, runtimeDeploymentSpec, NAMESPACE, dep =>
                {
                    string minAvailabilityStatus = dep.Status.Conditions.Where(cond => string.Equals(cond.Type, "Available")).First()?.Status;
                    Console.WriteLine("Deployment's minAvailabilityStatus = {0}", minAvailabilityStatus);
                    return string.Equals(minAvailabilityStatus, "True");
                });

                // Create load balancer for the deployed app
                runtimeService = await k8sHelpers.CreateServiceAndWait(fixture.Client, Specs.RunTimeService.GetSpec(appName + fixture.BuildNumber), NAMESPACE,
                    svc => svc.Status.LoadBalancer.Ingress != null && svc.Status.LoadBalancer.Ingress.Count > 0);
                Console.WriteLine("Load balancer started");

                // Ping the app to verify its availability
                var appIp = runtimeService.Status.LoadBalancer.Ingress.First().Ip;
                _output.WriteLine(string.Format("Your app {0} is available at {1}", appName, runtimeService.Status.LoadBalancer.Ingress.First().Ip));

                var response = await GetUrlAsync("http://" + appIp);
                Assert.True(response.IsSuccessStatusCode);
                _output.WriteLine(await response.Content.ReadAsStringAsync());
            }
            finally
            {
                if (fixture.Client != null)
                {
                    if (runtimeDeployment != null)
                    {
                        fixture.Client.DeleteNamespacedDeployment(new V1DeleteOptions(), runtimeDeployment.Metadata.Name, NAMESPACE);
                    }
                    if (runtimeService != null)
                    {
                        fixture.Client.DeleteNamespacedService(new V1DeleteOptions(), runtimeService.Metadata.Name, NAMESPACE);
                    }
                }
            }
        }

        /// <summary>
        /// Executes an HTTP GET request to the given URL, with a random-interval retry mechanism.
        /// </summary>
        /// <param name="url">URL to GET</param>
        /// <param name="retries">Maximum number of request attempts</param>
        /// <returns>HTTP response</returns>
        private async static Task<HttpResponseMessage> GetUrlAsync(string url, int retries = 4)
        {
            Random rand = new Random();
            Exception lastExc = null;
            while (retries > 0)
            {
                try
                {
                    return await httpClient.GetAsync(url);
                }
                catch (Exception exc)
                {
                    if (exc is HttpRequestException || exc is OperationCanceledException)
                    {
                        lastExc = exc;
                        --retries;

                        int interval = rand.Next(1000, 4000);
                        Console.WriteLine("GET failed: {0}", exc.Message);
                        Console.WriteLine("Retrying in {0}ms ({1} retries left)...", interval, retries);
                        await Task.Delay(interval);
                    }
                    else
                    {
                        Console.WriteLine("GET failed with unrecoverable error: {0}", exc.Message);
                        throw exc;
                    }
                }
            }
            throw lastExc;
        }
    }
}
