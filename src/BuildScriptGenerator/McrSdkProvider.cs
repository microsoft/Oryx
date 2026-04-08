// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Pulls SDKs from MCR/ACR container images and caches the tarball locally
    /// so existing platform installation logic can use it.
    /// </summary>
    /// <remarks>
    /// Image convention: {baseUrl}/{platformName}:{version}-{debianFlavor}
    /// The image contains a single tarball at /sdk/{platformName}-{debianFlavor}-{version}.tar.gz
    /// which is copied to the ExternalSdksStorageDir used by the existing installer scripts.
    /// </remarks>
    public class McrSdkProvider : IMcrSdkProvider
    {
        private const string DefaultMcrSdkImageBaseUrl = "mcr.microsoft.com/oryx/sdks";
        private const int ProcessTimeoutSeconds = 300;
        private readonly ILogger<McrSdkProvider> logger;
        private readonly IStandardOutputWriter outputWriter;
        private readonly BuildScriptGeneratorOptions commonOptions;

        public McrSdkProvider(
            IStandardOutputWriter outputWriter,
            ILogger<McrSdkProvider> logger,
            IOptions<BuildScriptGeneratorOptions> commonOptions)
        {
            this.logger = logger;
            this.outputWriter = outputWriter;
            this.commonOptions = commonOptions.Value;
        }

        /// <inheritdoc />
        public async Task<bool> PullSdkAsync(string platformName, string version, string debianFlavor)
        {
            var baseUrl = string.IsNullOrEmpty(this.commonOptions.McrSdkImageBaseUrl)
                ? DefaultMcrSdkImageBaseUrl
                : this.commonOptions.McrSdkImageBaseUrl;

            var imageTag = $"{baseUrl}/{platformName}:{version}-{debianFlavor}";
            var blobName = BlobNameHelper.GetBlobNameForVersion(platformName, version, debianFlavor);
            var containerSdkPath = $"/sdk/{blobName}";
            var localDir = Path.Combine(ExternalSdkProvider.ExternalSdksStorageDir, platformName);
            var localPath = Path.Combine(localDir, blobName);

            this.logger.LogInformation(
                "MCR SDK provider: pulling image {imageTag} for platform {platformName} version {version}",
                imageTag,
                platformName,
                version);
            this.outputWriter.WriteLine($"MCR SDK provider: pulling image {imageTag}");

            string containerId = null;
            try
            {
                // 1. docker pull
                var pullSuccess = await this.RunDockerCommandAsync($"pull {imageTag}");
                if (!pullSuccess)
                {
                    this.logger.LogError("MCR SDK provider: failed to pull image {imageTag}", imageTag);
                    this.outputWriter.WriteLine($"MCR SDK provider: failed to pull image {imageTag}");
                    return false;
                }

                // 2. docker create (returns container ID)
                containerId = await this.RunDockerCommandForOutputAsync($"create {imageTag} /bin/true");
                if (string.IsNullOrEmpty(containerId))
                {
                    this.logger.LogError("MCR SDK provider: failed to create container from {imageTag}", imageTag);
                    this.outputWriter.WriteLine($"MCR SDK provider: failed to create container from {imageTag}");
                    return false;
                }

                containerId = containerId.Trim();

                // 3. Ensure target directory exists
                Directory.CreateDirectory(localDir);

                // 4. docker cp
                var cpSuccess = await this.RunDockerCommandAsync($"cp {containerId}:{containerSdkPath} {localPath}");
                if (!cpSuccess)
                {
                    this.logger.LogError(
                        "MCR SDK provider: failed to copy SDK from container {containerId}:{containerSdkPath} to {localPath}",
                        containerId,
                        containerSdkPath,
                        localPath);
                    this.outputWriter.WriteLine($"MCR SDK provider: failed to copy SDK from container");
                    return false;
                }

                if (!File.Exists(localPath))
                {
                    this.logger.LogError("MCR SDK provider: expected file {localPath} not found after docker cp", localPath);
                    return false;
                }

                this.logger.LogInformation(
                    "MCR SDK provider: successfully cached SDK at {localPath}",
                    localPath);
                this.outputWriter.WriteLine($"MCR SDK provider: SDK cached at {localPath}");
                return true;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "MCR SDK provider: error pulling SDK for {platformName} {version}", platformName, version);
                this.outputWriter.WriteLine($"MCR SDK provider: error - {ex.Message}");
                return false;
            }
            finally
            {
                // 5. cleanup: docker rm
                if (!string.IsNullOrEmpty(containerId))
                {
                    try
                    {
                        await this.RunDockerCommandAsync($"rm -f {containerId}");
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogWarning(ex, "MCR SDK provider: failed to remove container {containerId}", containerId);
                    }
                }
            }
        }

        private async Task<bool> RunDockerCommandAsync(string arguments)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(psi);
            if (process == null)
            {
                this.logger.LogError("MCR SDK provider: failed to start docker process with args: {args}", arguments);
                return false;
            }

            var completed = await Task.Run(() => process.WaitForExit(ProcessTimeoutSeconds * 1000));
            if (!completed)
            {
                this.logger.LogError("MCR SDK provider: docker command timed out: docker {args}", arguments);
                try
                {
                    process.Kill();
                }
                catch
                {
                    // Best effort kill
                }

                return false;
            }

            if (process.ExitCode != 0)
            {
                var stderr = await process.StandardError.ReadToEndAsync();
                this.logger.LogError(
                    "MCR SDK provider: docker {args} failed with exit code {exitCode}. stderr: {stderr}",
                    arguments,
                    process.ExitCode,
                    stderr);
                return false;
            }

            return true;
        }

        private async Task<string> RunDockerCommandForOutputAsync(string arguments)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(psi);
            if (process == null)
            {
                this.logger.LogError("MCR SDK provider: failed to start docker process with args: {args}", arguments);
                return null;
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var completed = await Task.Run(() => process.WaitForExit(ProcessTimeoutSeconds * 1000));
            if (!completed)
            {
                this.logger.LogError("MCR SDK provider: docker command timed out: docker {args}", arguments);
                try
                {
                    process.Kill();
                }
                catch
                {
                    // Best effort kill
                }

                return null;
            }

            if (process.ExitCode != 0)
            {
                var stderr = await process.StandardError.ReadToEndAsync();
                this.logger.LogError(
                    "MCR SDK provider: docker {args} failed with exit code {exitCode}. stderr: {stderr}",
                    arguments,
                    process.ExitCode,
                    stderr);
                return null;
            }

            return output;
        }
    }
}
