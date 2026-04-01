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
    /// Provides SDK download functionality by pulling Docker images from MCR
    /// and extracting SDK tarballs from them.
    /// </summary>
    public class McrSdkProvider : IMcrSdkProvider
    {
        private const int DockerCommandTimeoutSeconds = 120;
        private readonly ILogger<McrSdkProvider> logger;
        private readonly IStandardOutputWriter outputWriter;
        private readonly BuildScriptGeneratorOptions commonOptions;

        public McrSdkProvider(
            IStandardOutputWriter outputWriter,
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            ILogger<McrSdkProvider> logger)
        {
            this.logger = logger;
            this.outputWriter = outputWriter;
            this.commonOptions = commonOptions.Value;
        }

        /// <inheritdoc />
        public async Task<bool> PullSdkAsync(string platformName, string version, string debianFlavor)
        {
            var imageBaseUrl = this.commonOptions.McrSdkImageBaseUrl;
            if (string.IsNullOrEmpty(imageBaseUrl))
            {
                imageBaseUrl = IMcrSdkProvider.DefaultMcrSdkImageBaseUrl;
            }

            var imageTag = $"{version}-{debianFlavor}";
            var imageReference = $"{imageBaseUrl}/{platformName}:{imageTag}";
            var blobName = BlobNameHelper.GetBlobNameForVersion(platformName, version, debianFlavor);
            var sdkCacheDir = Path.Combine(ExternalSdkProvider.ExternalSdksStorageDir, platformName);
            var targetFilePath = Path.Combine(sdkCacheDir, blobName);

            // Check if the SDK tarball already exists in the cache
            if (File.Exists(targetFilePath))
            {
                this.logger.LogInformation(
                    "SDK tarball for platform {platformName} version {version} already exists at {targetFilePath}. Skipping MCR pull.",
                    platformName,
                    version,
                    targetFilePath);
                return true;
            }

            this.logger.LogInformation(
                "Pulling SDK for platform {platformName} version {version} from MCR image {imageReference}",
                platformName,
                version,
                imageReference);
            this.outputWriter.WriteLine(
                $"Pulling SDK for platform {platformName} version {version} from MCR image {imageReference}...");

            string containerId = null;
            try
            {
                // Ensure the cache directory exists
                Directory.CreateDirectory(sdkCacheDir);

                // Step 1: Pull the Docker image
                var pullResult = await this.RunDockerCommandAsync($"pull {imageReference}");
                if (!pullResult.Success)
                {
                    this.logger.LogError(
                        "Failed to pull MCR image {imageReference}. Exit code: {exitCode}, Error: {error}",
                        imageReference,
                        pullResult.ExitCode,
                        pullResult.StdErr);
                    return false;
                }

                // Step 2: Create a container from the image (without starting it)
                var createResult = await this.RunDockerCommandAsync($"create {imageReference}");
                if (!createResult.Success || string.IsNullOrWhiteSpace(createResult.StdOut))
                {
                    this.logger.LogError(
                        "Failed to create container from MCR image {imageReference}. Exit code: {exitCode}, Error: {error}",
                        imageReference,
                        createResult.ExitCode,
                        createResult.StdErr);
                    return false;
                }

                containerId = createResult.StdOut.Trim();

                // Step 3: Copy the SDK tarball from the container
                var sourcePathInContainer = $"{IMcrSdkProvider.SdkDirectoryInImage}/{blobName}";
                var cpResult = await this.RunDockerCommandAsync($"cp {containerId}:{sourcePathInContainer} {targetFilePath}");
                if (!cpResult.Success)
                {
                    this.logger.LogError(
                        "Failed to copy SDK tarball from container {containerId} path {sourcePathInContainer}. Exit code: {exitCode}, Error: {error}",
                        containerId,
                        sourcePathInContainer,
                        cpResult.ExitCode,
                        cpResult.StdErr);
                    return false;
                }

                // Verify the file was actually copied
                if (!File.Exists(targetFilePath))
                {
                    this.logger.LogError(
                        "SDK tarball was not found at {targetFilePath} after docker cp operation.",
                        targetFilePath);
                    return false;
                }

                this.logger.LogInformation(
                    "Successfully pulled SDK for platform {platformName} version {version} from MCR to {targetFilePath}",
                    platformName,
                    version,
                    targetFilePath);
                this.outputWriter.WriteLine(
                    $"Successfully pulled SDK for platform {platformName} version {version} from MCR.");

                return true;
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    ex,
                    "Error pulling SDK for platform {platformName} version {version} from MCR image {imageReference}",
                    platformName,
                    version,
                    imageReference);
                this.outputWriter.WriteLine(
                    $"Error pulling SDK for platform {platformName} version {version} from MCR: {ex.Message}");

                // Clean up any partially downloaded file
                if (File.Exists(targetFilePath))
                {
                    try
                    {
                        File.Delete(targetFilePath);
                    }
                    catch (Exception cleanupEx)
                    {
                        this.logger.LogWarning(cleanupEx, "Failed to clean up partial SDK file at {targetFilePath}", targetFilePath);
                    }
                }

                return false;
            }
            finally
            {
                // Step 4: Remove the container
                if (!string.IsNullOrEmpty(containerId))
                {
                    try
                    {
                        await this.RunDockerCommandAsync($"rm {containerId}");
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogWarning(ex, "Failed to remove container {containerId}", containerId);
                    }
                }
            }
        }

        private async Task<DockerCommandResult> RunDockerCommandAsync(string arguments)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            this.logger.LogDebug("Running docker command: docker {arguments}", arguments);

            using var process = new Process { StartInfo = processStartInfo };
            process.Start();

            var stdOutTask = process.StandardOutput.ReadToEndAsync();
            var stdErrTask = process.StandardError.ReadToEndAsync();

            var completed = await Task.Run(() => process.WaitForExit(DockerCommandTimeoutSeconds * 1000));
            if (!completed)
            {
                this.logger.LogError("Docker command timed out after {timeout} seconds: docker {arguments}", DockerCommandTimeoutSeconds, arguments);
                try
                {
                    process.Kill();
                }
                catch
                {
                    // Best effort kill
                }

                return new DockerCommandResult
                {
                    Success = false,
                    ExitCode = -1,
                    StdOut = string.Empty,
                    StdErr = $"Command timed out after {DockerCommandTimeoutSeconds} seconds",
                };
            }

            var stdOut = await stdOutTask;
            var stdErr = await stdErrTask;

            return new DockerCommandResult
            {
                Success = process.ExitCode == 0,
                ExitCode = process.ExitCode,
                StdOut = stdOut,
                StdErr = stdErr,
            };
        }

        private class DockerCommandResult
        {
            public bool Success { get; set; }

            public int ExitCode { get; set; }

            public string StdOut { get; set; }

            public string StdErr { get; set; }
        }
    }
}
