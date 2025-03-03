// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildScriptGenerator
{
  /// <summary>
  /// Interface for external SDK provider that communicates with an external service
  /// to request and download SDKs for various platforms.
  /// </summary>
  public interface IExternalSdkProvider
  {
    /// <summary>
    /// Checks if external SDK provider is enabled.
    /// </summary>
    /// <returns>True if external SDK provider should be used.</returns>
    bool IsEnabled();

    /// <summary>
    /// Gets available versions for a specific platform from the external SDK provider.
    /// </summary>
    /// <param name="platformName">The name of the platform.</param>
    /// <returns>A list of available versions.</returns>
    Task<List<string>> GetAvailableVersionsAsync(string platformName);

    /// <summary>
    /// Requests an SDK from the external provider and returns the path where it is stored.
    /// </summary>
    /// <param name="platformName">The name of the platform.</param>
    /// <param name="version">The version of the SDK.</param>
    /// <returns>The path where the SDK is stored locally.</returns>
    Task<string> RequestSdkAsync(string platformName, string version);
  }
}
