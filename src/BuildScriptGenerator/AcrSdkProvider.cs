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
    /// Fetches SDK tarballs directly from an OCI container registry.
    /// SDK images are single-layer <c>FROM scratch</c> images where
    /// the layer IS the SDK tarball.
    /// </summary>
    /// <remarks>
    /// Makes direct HTTP calls to the registry (no Unix socket).
    /// See <see cref="ExternalAcrSdkProvider"/> for the socket-based variant.
    /// </remarks>
    public class AcrSdkProvider : IAcrSdkProvider
    {
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

            var repository = SdkStorageConstants.GetSdkImageRepository(platformName, this.options.OryxAcrSdkRepositoryPrefix);
            var tag = $"{debianFlavor}-{version}";
            var blobName = $"{platformName}-{debianFlavor}-{version}.tar.gz";

            this.logger.LogInformation(
                "Requesting SDK from ACR: {Repository}:{Tag}",
                repository,
                tag);
            this.outputWriter.WriteLine(
                $"Requesting SDK from ACR: {repository}:{tag}");

            // Download to the writable dynamic install directory, NOT /var/OryxSdks (read-only external mount).
            var downloadDir = Path.Combine(this.options.DynamicInstallRootDir, platformName);
            var tarballPath = Path.Combine(downloadDir, blobName);

            if (File.Exists(tarballPath))
            {
                this.logger.LogInformation(
                    "SDK tarball already downloaded at {FilePath}, skipping ACR pull.",
                    tarballPath);
                this.outputWriter.WriteLine(
                    $"SDK tarball already downloaded at {tarballPath}");
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
                Directory.CreateDirectory(downloadDir);

                var success = await this.ociClient.DownloadLayerBlobAsync(
                    repository,
                    layerDigest,
                    tarballPath);

                if (success)
                {
                    this.logger.LogInformation(
                        "Successfully pulled SDK from ACR: {Repository}:{Tag} → {FilePath}",
                        repository,
                        tag,
                        tarballPath);
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
