// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Common;

namespace Microsoft.Oryx.BuildScriptGenerator
{
  public class ExternalSdkStorageVersionProviderBase
  {
    private readonly ILogger logger;
    private readonly BuildScriptGeneratorOptions commonOptions;
    private readonly IExternalSdkProvider externalProvider;

    public ExternalSdkStorageVersionProviderBase(
        IOptions<BuildScriptGeneratorOptions> commonOptions,
        IExternalSdkProvider externalSdkProvider,
        ILoggerFactory loggerFactory)
    {
      this.commonOptions = commonOptions.Value;
      this.logger = loggerFactory.CreateLogger(this.GetType());
      this.externalProvider = externalSdkProvider;
    }

    /// <summary>
    /// Gets the list of all blobs in the <paramref name="platformName"/> storage container using an external sdk provider and determines
    /// the supported and default versions.
    /// -----------
    /// We determine what versions are available differently based on the OS type where the oryx
    /// command was run.
    /// For <see cref="OsTypes.DebianStretch"/> we use the existance of <see cref="SdkStorageConstants.LegacySdkVersionMetadataName"/>
    /// metadata as the indicator for a supported version.
    /// For other <see cref="OsTypes"/> we use both <see cref="SdkStorageConstants.SdkVersionMetadataName"/> and
    /// matching <see cref="SdkStorageConstants.OsTypeMetadataName"/> metadata to indicate a matching version.
    /// </summary>
    /// <param name="platformName">Name of the platform to get the supported versions for</param>
    /// <returns><see cref="PlatformVersionInfo"/> containing supported and default versions</returns>
    protected PlatformVersionInfo GetAvailableVersionsFromExternalProvider(string platformName)
    {
      this.logger.LogInformation("Getting list of available versions for platform {platformName}, via external sdk provider", platformName);
      var xdoc = this.externalProvider.GetPlatformMetaDataAsync(platformName).Result;
      var supportedVersions = new List<string>();

      var isStretch = string.Equals(this.commonOptions.DebianFlavor, OsTypes.DebianStretch, StringComparison.OrdinalIgnoreCase);

      var sdkVersionMetadataName = isStretch
          ? SdkStorageConstants.LegacySdkVersionMetadataName
          : SdkStorageConstants.SdkVersionMetadataName;
      foreach (var metadataElement in xdoc.XPathSelectElements($"//Blobs/Blob/Metadata"))
      {
        var childElements = metadataElement.Elements();
        var versionElement = childElements
            .Where(e => string.Equals(sdkVersionMetadataName, e.Name.LocalName, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();

        var osTypeElement = childElements
            .Where(e => string.Equals(SdkStorageConstants.OsTypeMetadataName, e.Name.LocalName, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();

        // if a matching version element is not found, we do not add as a supported version
        // if the os type is stretch and we find a blob with a 'Version' metadata, we know it is a supported version
        // otherwise, we check the blob for 'Sdk_version' metadata AND ensure 'Os_type' metadata matches current debianFlavor
        if (versionElement != null &&
            (isStretch || (osTypeElement != null && string.Equals(this.commonOptions.DebianFlavor, osTypeElement.Value, StringComparison.OrdinalIgnoreCase))))
        {
          supportedVersions.Add(versionElement.Value);
        }
      }

      var defaultVersion = this.GetDefaultVersion(platformName);
      return PlatformVersionInfo.CreateAvailableViaExternalProvider(supportedVersions, defaultVersion);
    }

    protected string GetDefaultVersion(string platformName)
    {
      var defaultFileBlobName = string.IsNullOrEmpty(this.commonOptions.DebianFlavor)
              || string.Equals(this.commonOptions.DebianFlavor, OsTypes.DebianStretch, StringComparison.OrdinalIgnoreCase)
          ? SdkStorageConstants.DefaultVersionFileName
          : $"{SdkStorageConstants.DefaultVersionFilePrefix}.{this.commonOptions.DebianFlavor}.{SdkStorageConstants.DefaultVersionFileType}";

      this.logger.LogDebug("Getting the default version for platform {platformName} by fetching blob: {defaultFileBlobName} via external provider", platformName, defaultFileBlobName);
      if (!this.externalProvider.RequestSdkAsync(platformName, defaultFileBlobName).Result)
      {
        throw new InvalidOperationException(
            $"Failed to get the default version for platform {platformName} by fetching blob: {defaultFileBlobName}");
      }

      // get default version from the file
      var defaultFilePath = Path.Combine(ExternalSdkProvider.ExternalSdksStorageDir, platformName, defaultFileBlobName);
      string defaultVersionString = null;
      string defaultVersion = null;
      try
      {
        defaultVersionString = File.ReadAllText(defaultFilePath);
        using (var stringReader = new StringReader(defaultVersionString))
        {
          string line;
          while ((line = stringReader.ReadLine()) != null)
          {
            // Ignore any comments in the file
            if (!line.StartsWith("#") || !line.StartsWith("//"))
            {
              defaultVersion = line.Trim();
              break;
            }
          }
        }
      }
      catch (Exception)
      {
        throw new InvalidOperationException($"Error reading default version file {defaultFilePath} for platform {platformName}");
      }

      this.logger.LogDebug("Got the default version for {platformName} as {defaultVersion}.", platformName, defaultVersion);

      if (string.IsNullOrEmpty(defaultVersion))
      {
        throw new InvalidOperationException("Default version cannot be empty.");
      }

      return defaultVersion;
    }
  }
}

