// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using System.Xml.Linq;

namespace Microsoft.Oryx.BuildScriptGenerator
{
  /// <summary>
  /// Interface for external SDK provider that communicates with an external service
  /// to request and download SDKs for various platforms.
  /// </summary>
  public interface IExternalSdkProvider
  {
    /// <summary>
    /// The directory where SDKs are cached by the external provider.
    /// </summary>
    public const string ExternalSdksStorageDir = "/var/OryxSdksCache";

    /// <summary>
    /// Gets all metadata for a specific platform from the external SDK provider.
    /// </summary>
    Task<XDocument> GetPlatformMetaDataAsync(string platformName);

    /// <summary>
    /// Gets the checksum for a specific version of a platform from the external SDK provider.
    /// </summary>
    Task<string> GetChecksumForVersionAsync(string platformName, string version);

    /// <summary>
    /// Requests an SDK to be downloaded to SDKs cache path by the external SDK provider.
    /// </summary>
    /// <param name="platformName">The name of the platform.</param>
    /// <param name="blobName">The blobName of the SDK.</param>
    /// <returns>Returns true if the download was successful</returns>
    Task<bool> RequestSdkAsync(string platformName, string blobName);
  }
}
