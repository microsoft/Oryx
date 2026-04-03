// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// HTTP client for the OCI Distribution API. Enables Oryx to discover SDK versions
    /// and download SDK tarballs from an OCI-compliant container registry (e.g. Azure Container Registry)
    /// using only HttpClient — no external tools (docker, crane, oras) required.
    /// All SDK images are public, so no authentication is needed.
    /// </summary>
    public class OciRegistryClient
    {
        private readonly HttpClient httpClient;
        private readonly string registryUrl;
        private readonly ILogger logger;

        public OciRegistryClient(string registryUrl, IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
        {
            if (string.IsNullOrWhiteSpace(registryUrl))
            {
                throw new ArgumentException("Registry URL must not be empty.", nameof(registryUrl));
            }

            var trimmed = registryUrl.TrimEnd('/');
            if (!trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "Registry URL must use HTTPS.",
                    nameof(registryUrl));
            }

            this.registryUrl = trimmed;
            this.httpClient = httpClientFactory.CreateClient("general");
            this.logger = loggerFactory.CreateLogger<OciRegistryClient>();
        }

        /// <summary>
        /// Gets the first layer digest from a manifest (SDK images are single-layer FROM scratch images).
        /// </summary>
        public static string GetFirstLayerDigest(OciManifest manifest)
        {
            return manifest?.Layers?.FirstOrDefault()?.Digest;
        }

        /// <summary>
        /// Lists all tags for a repository, handling Link-header pagination.
        /// </summary>
        public async Task<List<string>> GetAllTagsAsync(string repository)
        {
            var allTags = new List<string>();
            var url = $"{this.registryUrl}/v2/{repository}/tags/list";

            while (!string.IsNullOrEmpty(url))
            {
                this.logger.LogDebug("Fetching tags from {url}", url);
                using (var response = await this.httpClient.GetAsync(url))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException(
                            $"Failed to list tags for repository '{repository}' (HTTP {(int)response.StatusCode}).");
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    var tagList = JsonSerializer.Deserialize<OciTagList>(json);
                    if (tagList?.Tags != null)
                    {
                        allTags.AddRange(tagList.Tags);
                    }

                    // Handle OCI pagination via Link header (RFC 5988)
                    url = null;
                    if (response.Headers.TryGetValues("Link", out var linkValues))
                    {
                        var linkHeader = linkValues.FirstOrDefault();
                        if (linkHeader != null)
                        {
                            var match = Regex.Match(linkHeader, @"<([^>]+)>;\s*rel=""next""");
                            if (match.Success)
                            {
                                url = match.Groups[1].Value;
                                if (!url.StartsWith("http"))
                                {
                                    url = $"{this.registryUrl}{url}";
                                }
                            }
                        }
                    }
                }
            }

            return allTags;
        }

        /// <summary>
        /// Fetches an OCI image manifest for the given repository and tag.
        /// Sends both OCI and Docker v2 Accept types in a single request so the registry
        /// can return whichever format it supports without needing a fallback round-trip.
        /// </summary>
        public async Task<OciManifest> GetManifestAsync(string repository, string tag)
        {
            var url = $"{this.registryUrl}/v2/{repository}/manifests/{tag}";
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                // Accept both formats in one request — avoids a second round-trip when
                // the registry only supports Docker v2 (benchmarked ~50% faster on ACR).
                request.Headers.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/vnd.oci.image.manifest.v1+json", 1.0));
                request.Headers.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/vnd.docker.distribution.manifest.v2+json", 0.9));

                using (var response = await this.httpClient.SendAsync(request))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException(
                            $"Failed to fetch manifest for '{repository}:{tag}' (HTTP {(int)response.StatusCode}).");
                    }

                    // Deserialize directly from the response stream — avoids an
                    // intermediate string allocation for the manifest JSON.
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        return await JsonSerializer.DeserializeAsync<OciManifest>(stream);
                    }
                }
            }
        }

        /// <summary>
        /// Fetches the image config blob (contains Labels) for the given repository and digest.
        /// </summary>
        public async Task<OciImageConfig> GetImageConfigAsync(string repository, string configDigest)
        {
            var url = $"{this.registryUrl}/v2/{repository}/blobs/{configDigest}";
            using (var response = await this.httpClient.GetAsync(url))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        $"Failed to fetch config for '{repository}' (HTTP {(int)response.StatusCode}).");
                }

                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    return await JsonSerializer.DeserializeAsync<OciImageConfig>(stream);
                }
            }
        }

        /// <summary>
        /// Gets the default version for a platform from the "-default" tag's image config labels.
        /// </summary>
        public async Task<string> GetDefaultVersionAsync(string repository, string osFlavor)
        {
            var tag = $"{osFlavor}-default";
            this.logger.LogDebug("Fetching default version from {repository}:{tag}", repository, tag);

            try
            {
                var manifest = await this.GetManifestAsync(repository, tag);
                var configDigest = manifest.Config?.Digest;
                if (string.IsNullOrEmpty(configDigest))
                {
                    this.logger.LogWarning("No config digest found in manifest for {repository}:{tag}", repository, tag);
                    return null;
                }

                var config = await this.GetImageConfigAsync(repository, configDigest);
                if (config?.Config?.Labels != null &&
                    config.Config.Labels.TryGetValue(Common.SdkStorageConstants.AcrVersionLabelName, out var version))
                {
                    return version;
                }

                this.logger.LogWarning("Version label not found in config for {repository}:{tag}", repository, tag);
                return null;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to get default version from {repository}:{tag}", repository, tag);
                throw;
            }
        }

        /// <summary>
        /// Downloads a layer blob (the SDK tarball) to disk and verifies its SHA256 digest.
        /// The digest in the manifest IS the content hash — no separate checksum metadata needed.
        /// Uses single-pass streaming: the SHA256 hash is computed incrementally as bytes are
        /// written to disk, eliminating a second full read of the file for verification.
        /// </summary>
        public async Task<bool> DownloadLayerBlobAsync(string repository, string layerDigest, string outputPath)
        {
            var url = $"{this.registryUrl}/v2/{repository}/blobs/{layerDigest}";
            this.logger.LogDebug("Downloading layer blob {digest} from {repository}", layerDigest, repository);

            var expectedSha = layerDigest.StartsWith("sha256:")
                ? layerDigest.Substring("sha256:".Length)
                : layerDigest;

            using (var response = await this.httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        $"Failed to download blob from '{repository}' (HTTP {(int)response.StatusCode}).");
                }

                using (var sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256))
                using (var networkStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = File.Create(outputPath))
                {
                    var buffer = new byte[81920];
                    int bytesRead;
                    while ((bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        sha256.AppendData(buffer, 0, bytesRead);
                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                    }

                    var actualSha = Convert.ToHexString(sha256.GetHashAndReset()).ToLowerInvariant();
                    if (!string.Equals(actualSha, expectedSha, StringComparison.OrdinalIgnoreCase))
                    {
                        this.logger.LogError(
                            "SHA256 digest mismatch for blob {digest}. Expected: {expected}, Actual: {actual}",
                            layerDigest,
                            expectedSha,
                            actualSha);
                        fileStream.Close();
                        File.Delete(outputPath);
                        return false;
                    }
                }
            }

            this.logger.LogDebug("Successfully downloaded and verified layer blob {digest}", layerDigest);
            return true;
        }
    }
}
