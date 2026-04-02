// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Common;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// ACR-based SDK provider that fetches SDK tarballs directly from an OCI-compliant
    /// container registry using the OCI Distribution API. SDK images are single-layer
    /// <c>FROM scratch</c> images where the layer IS the SDK tarball.
    /// </summary>
    /// <remarks>
    /// This provider makes direct HTTP calls to the registry — no Unix socket, no LWAS
    /// intermediary. A future phase will add socket→LWAS→ACR support in the existing
    /// <see cref="ExternalSdkProvider"/> path.
    /// </remarks>
    public class AcrSdkProvider : IAcrSdkProvider
    {
        /// <summary>
        /// The directory where SDKs are cached (same as the blob-based provider).
        /// </summary>
        public const string SdksCacheDir = "/var/OryxSdks";

        private readonly ILogger<AcrSdkProvider> logger;
        private readonly IStandardOutputWriter outputWriter;
        private readonly BuildScriptGeneratorOptions options;
        private readonly OciRegistryClient ociClient;

        public AcrSdkProvider(
            IStandardOutputWriter outputWriter,
            ILogger<AcrSdkProvider> logger,
            IOptions<BuildScriptGeneratorOptions> options,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory)
        {
            this.logger = logger;
            this.outputWriter = outputWriter;
            this.options = options.Value;

            var registryUrl = string.IsNullOrEmpty(this.options.OryxAcrSdkRegistryUrl)
                ? SdkStorageConstants.DefaultAcrSdkRegistryUrl
                : this.options.OryxAcrSdkRegistryUrl;

            this.ociClient = new OciRegistryClient(registryUrl, httpClientFactory, loggerFactory);
        }

        /// <inheritdoc/>
        public async Task<bool> RequestSdkFromAcrAsync(string platformName, string version, string debianFlavor)
        {
            if (string.IsNullOrEmpty(platformName))
            {
                throw new ArgumentException("Platform name cannot be null or empty.", nameof(platformName));
            }

            if (string.IsNullOrEmpty(version))
            {
                throw new ArgumentException("Version cannot be null or empty.", nameof(version));
            }

            if (string.IsNullOrEmpty(debianFlavor))
            {
                debianFlavor = this.options.DebianFlavor ?? "bookworm";
            }

            var repository = $"{SdkStorageConstants.AcrSdkRepositoryPrefix}/{platformName}";
            var tag = $"{debianFlavor}-{version}";
            var blobName = $"{platformName}-{debianFlavor}-{version}.tar.gz";

            this.logger.LogInformation(
                "Requesting SDK from ACR: {Repository}:{Tag}",
                repository,
                tag);
            this.outputWriter.WriteLine(
                $"Requesting SDK from ACR: {repository}:{tag}");

            // Check if the file is already cached locally
            var expectedFilePath = Path.Combine(SdksCacheDir, platformName, blobName);
            if (File.Exists(expectedFilePath))
            {
                this.logger.LogInformation(
                    "SDK already cached at {FilePath}, skipping ACR pull.",
                    expectedFilePath);
                this.outputWriter.WriteLine(
                    $"SDK already cached at {expectedFilePath}");
                return true;
            }

            try
            {
                // 1. Get manifest → extract single layer digest
                var manifest = await this.ociClient.GetManifestAsync(repository, tag);
                var layerDigest = OciRegistryClient.GetFirstLayerDigest(manifest);

                if (string.IsNullOrEmpty(layerDigest))
                {
                    this.logger.LogWarning(
                        "No layer found in manifest for {Repository}:{Tag}",
                        repository,
                        tag);
                    return false;
                }

                // 2. Download the layer blob (the SDK tarball) and verify its SHA256 digest
                var platformDir = Path.Combine(SdksCacheDir, platformName);
                Directory.CreateDirectory(platformDir);

                var success = await this.ociClient.DownloadLayerBlobAsync(
                    repository,
                    layerDigest,
                    expectedFilePath);

                if (success)
                {
                    this.logger.LogInformation(
                        "Successfully pulled SDK from ACR: {Repository}:{Tag} → {FilePath}",
                        repository,
                        tag,
                        expectedFilePath);
                    this.outputWriter.WriteLine(
                        $"Successfully pulled SDK from ACR: {platformName} {version}");
                    return true;
                }
                else
                {
                    this.logger.LogWarning(
                        "ACR SDK pull failed digest verification: {Repository}:{Tag}",
                        repository,
                        tag);
                    this.outputWriter.WriteLine(
                        $"Failed to pull SDK from ACR (digest mismatch): {platformName} {version}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    ex,
                    "Error pulling SDK from ACR: {Repository}:{Tag}",
                    repository,
                    tag);
                this.outputWriter.WriteLine(
                    $"Error pulling SDK from ACR: {platformName} {version}: {ex.Message}");
                return false;
            }
        }
    }
}
