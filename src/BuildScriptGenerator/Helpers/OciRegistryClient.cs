// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
            this.registryUrl = registryUrl.TrimEnd('/');
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
        /// Replaces <see cref="ListBlobsHelper.GetAllBlobs"/> for ACR-based discovery.
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
                        throw new HttpRequestException($"Request to {url} failed with status code {response.StatusCode}");
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
        /// Tries OCI manifest format first, falls back to Docker manifest v2.
        /// </summary>
        public async Task<OciManifest> GetManifestAsync(string repository, string tag)
        {
            var url = $"{this.registryUrl}/v2/{repository}/manifests/{tag}";
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                request.Headers.Add("Accept", "application/vnd.oci.image.manifest.v1+json");

                using (var response = await this.httpClient.SendAsync(request))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        // Fall back to Docker manifest v2
                        using (var fallbackRequest = new HttpRequestMessage(HttpMethod.Get, url))
                        {
                            fallbackRequest.Headers.Add("Accept", "application/vnd.docker.distribution.manifest.v2+json");
                            using (var fallbackResponse = await this.httpClient.SendAsync(fallbackRequest))
                            {
                                if (!fallbackResponse.IsSuccessStatusCode)
                                {
                                    throw new HttpRequestException($"Fallback request to {url} failed with status code {fallbackResponse.StatusCode}");
                                }

                                var fallbackJson = await fallbackResponse.Content.ReadAsStringAsync();
                                return JsonSerializer.Deserialize<OciManifest>(fallbackJson);
                            }
                        }
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<OciManifest>(json);
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
                    throw new HttpRequestException($"Request to {url} failed with status code {response.StatusCode}");
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<OciImageConfig>(json);
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
        /// </summary>
        public async Task<bool> DownloadLayerBlobAsync(string repository, string layerDigest, string outputPath)
        {
            var url = $"{this.registryUrl}/v2/{repository}/blobs/{layerDigest}";
            this.logger.LogDebug("Downloading layer blob {digest} from {repository}", layerDigest, repository);

            using (var response = await this.httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Request to {url} failed with status code {response.StatusCode}");
                }

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = File.Create(outputPath))
                {
                    await stream.CopyToAsync(fileStream);
                }
            }

            // Verify SHA256 digest
            var expectedSha = layerDigest.StartsWith("sha256:")
                ? layerDigest.Substring("sha256:".Length)
                : layerDigest;

            using (var fileStream = File.OpenRead(outputPath))
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(fileStream);
                var actualSha = BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLowerInvariant();

                if (!string.Equals(actualSha, expectedSha, StringComparison.OrdinalIgnoreCase))
                {
                    this.logger.LogError(
                        "SHA256 digest mismatch for {repository} blob {digest}. Expected: {expected}, Actual: {actual}",
                        repository,
                        layerDigest,
                        expectedSha,
                        actualSha);
                    File.Delete(outputPath);
                    return false;
                }
            }

            this.logger.LogDebug("Successfully downloaded and verified layer blob {digest}", layerDigest);
            return true;
        }
    }
}
