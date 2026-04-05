// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Common;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Fetches SDK tarballs directly from an OCI container registry.
    /// SDK images are single-layer <c>FROM scratch</c> images containing a single
    /// <c>.tar.gz</c> SDK file. The OCI layer blob is a tar archive of the image
    /// filesystem, so this provider downloads the layer, extracts the inner SDK
    /// tarball from it, and caches it locally.
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
        public async Task<bool> RequestSdkFromAcrAsync(string platformName, string version, string debianFlavor, string runtimeVersion = null)
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
            var tag = string.IsNullOrEmpty(runtimeVersion)
                ? $"{debianFlavor}-{version}"
                : $"{debianFlavor}-{version}_{runtimeVersion}";
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
                    "SDK tarball already cached at {FilePath}, skipping ACR pull.",
                    tarballPath);
                this.outputWriter.WriteLine(
                    $"SDK tarball already cached at {tarballPath}");
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
                    this.outputWriter.WriteLine($"No layer found in ACR manifest for {platformName} {version}.");
                    return false;
                }

                Directory.CreateDirectory(downloadDir);

                // 2. Download the OCI layer blob to a temp file.
                //    The layer is a tar archive of the image filesystem (not the SDK tarball itself).
                var layerTempPath = Path.Combine(downloadDir, $".layer-{Guid.NewGuid():N}.tmp");
                try
                {
                    var downloadSuccess = await this.ociClient.DownloadLayerBlobAsync(
                        repository,
                        layerDigest,
                        layerTempPath);

                    if (!downloadSuccess)
                    {
                        this.logger.LogWarning(
                            "ACR SDK pull failed digest verification: {Repository}:{Tag}",
                            repository,
                            tag);
                        this.outputWriter.WriteLine(
                            $"Failed to pull SDK from ACR (digest mismatch): {platformName} {version}");
                        return false;
                    }

                    // 3. Extract the inner SDK .tar.gz from the layer tar.
                    //    The image is FROM scratch with a single COPY of the SDK tarball,
                    //    so the layer contains the .tar.gz as a top-level entry.
                    this.ExtractFileFromTar(layerTempPath, tarballPath, blobName);
                }
                finally
                {
                    // Always clean up the temporary layer file
                    if (File.Exists(layerTempPath))
                    {
                        File.Delete(layerTempPath);
                    }
                }

                this.logger.LogInformation(
                    "Successfully pulled SDK from ACR: {Repository}:{Tag} → {FilePath}",
                    repository,
                    tag,
                    tarballPath);
                this.outputWriter.WriteLine(
                    $"Successfully pulled SDK from ACR: {platformName} {version}");
                return true;
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

        /// <summary>
        /// Extracts the expected SDK .tar.gz file from an OCI layer tar archive.
        /// OCI layers use media type "application/vnd.docker.image.rootfs.diff.tar.gzip",
        /// so the blob must be decompressed before reading tar entries.
        /// </summary>
        private void ExtractFileFromTar(string layerPath, string outputPath, string expectedFileName)
        {
            using (var stream = File.OpenRead(layerPath))
            using (var gzipStream = new GZipStream(stream, CompressionMode.Decompress))
            using (var tarReader = new TarReader(gzipStream))
            {
                TarEntry entry;
                while ((entry = tarReader.GetNextEntry()) != null)
                {
                    var name = entry.Name.TrimStart('.', '/');
                    if (entry.DataStream != null && name.Equals(expectedFileName, StringComparison.OrdinalIgnoreCase))
                    {
                        entry.ExtractToFile(outputPath, overwrite: true);
                        return;
                    }
                }
            }

            throw new InvalidOperationException($"Expected entry '{expectedFileName}' not found in OCI layer: {layerPath}");
        }
    }
}
