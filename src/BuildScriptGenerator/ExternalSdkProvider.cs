// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGenerator
{
  /// <summary>
  /// Provides SDK download functionality by communicating with an external service via Unix domain socket.
  /// </summary>
  public class ExternalSdkProvider : IExternalSdkProvider
  {
    public const string ExternalSdksStorageDir = "/var/OryxSdksCache";
    private const string SocketPath = "/var/OryxSdks/oryx-pull-sdk.socket";
    private readonly BuildScriptGeneratorOptions options;
    private readonly ILogger<ExternalSdkProvider> logger;

    public ExternalSdkProvider(
        IOptions<BuildScriptGeneratorOptions> options,
        ILogger<ExternalSdkProvider> logger)
    {
      this.options = options.Value;
      this.logger = logger;
    }

    /// <inheritdoc />
    public bool IsEnabled()
    {
      return this.options.EnableExternalSdkProvider;
    }

    /// <inheritdoc />
    public async Task<XDocument> GetPlatformMetaDataAsync(string platformName)
    {
      try
      {
        var request = new SdkProviderRequest
        {
          PlatformName = platformName,
          UrlParameters = new Dictionary<string, string>
          {
            { "restype", "container" },
            { "comp", "list" },
            { "include", "metadata" },
          },
        };

        var response = await this.SendRequestAsync(request);
        var filePath = Path.Combine(ExternalSdksStorageDir, platformName, platformName);

        if (response && File.Exists(filePath))
        {
          this.logger.LogInformation(
              "Successfully got metadata for platform {platformName}, available at {FilePath}", platformName, filePath);

          // TODO: Error handling
          return XDocument.Load(filePath);
        }
        else
        {
          this.logger.LogError("Failed to get metadata for platform {platformName}", platformName);
          return null;
        }
      }
      catch (Exception ex)
      {
        this.logger.LogError(ex, "Error getting metadata for platform {platformName} from external provider.", platformName);
        return null;
      }
    }

    /// <inheritdoc />
    public async Task<string> GetChecksumForVersionAsync(string platformName, string blobName)
    {
      try
      {
        var xdoc = await this.GetPlatformMetaDataAsync(platformName);
        if (xdoc == null)
        {
          return null;
        }
        else
        {
          var blobElement = xdoc.XPathSelectElement($"//Blobs/Blob[Name='{blobName}']");
          var checksum = blobElement?.Element("Metadata")?.Element("Checksum")?.Value;
          if (string.IsNullOrEmpty(checksum))
          {
            this.logger.LogError("Checksum not found for platform {platformName}, blobname {blobName} from external provider.", platformName, blobName);
          }

          return checksum;
        }
      }
      catch (Exception ex)
      {
        this.logger.LogError(ex, "Error getting blob checksum for platform {platformName}, blobname {blobName} from external provider.", platformName, blobName);
        return null;
      }
    }

    public bool CheckLocalCacheForSdk(string platformName, string blobName, string expectedChecksum)
    {
      var sdkDir = Path.Combine(ExternalSdksStorageDir, platformName);
      var sdkPath = Path.Combine(sdkDir, blobName);
      if (File.Exists(sdkPath))
      {
        var actualChecksum = this.GetSHA512Checksum(sdkPath);
        if (expectedChecksum == actualChecksum)
        {
          this.logger.LogInformation("SDK for platform {platformName}, blobName {blobName} already exists in cache at {sdkPath} and checksum matches", platformName, blobName, sdkPath);
          return true;
        }
        else
        {
          this.logger.LogWarning("SDK for platform {platformName}, blobName {blobName} already exists in cache at {sdkPath} but checksum does not match, considering the blob as corrupted", platformName, blobName, sdkPath);
        }
      }

      return false;
    }

    /// <inheritdoc />
    public async Task<bool> RequestSdkAsync(string platformName, string blobName)
    {
      // Get the expected checksum for the  SDK
      var expectedChecksum = await this.GetChecksumForVersionAsync(platformName, blobName);
      if (expectedChecksum == null)
      {
        this.logger.LogError("Failed to get checksum for blob : platform {platformName}, blobName {blobName}. Cannot download SDK.", platformName, blobName);
        return false;
      }

      if (this.CheckLocalCacheForSdk(platformName, blobName, expectedChecksum))
      {
        return true;
      }
      else
      {
        try
        {
          var request = new SdkProviderRequest
          {
            PlatformName = platformName,
            BlobName = blobName,
          };

          var response = await this.SendRequestAsync(request);
          var filePath = Path.Combine(ExternalSdksStorageDir, platformName, blobName);

          if (response && File.Exists(filePath))
          {
            this.logger.LogInformation(
                "Successfully requested SDK for platform {platformName}, blobName {blobName}, available at {FilePath}", platformName, blobName, filePath);
          }
          else
          {
            this.logger.LogError(
                "Failed to get SDK for platform {platformName}, blobName {blobName}", platformName, blobName);
            return false;
          }
        }
        catch (Exception ex)
        {
          this.logger.LogError(ex, "Error requesting SDK for platform {platformName} blobName {blobName} from external provider.", platformName, blobName);
          return false;
        }
      }

      // Verify checksum
      var sdkDir = Path.Combine(ExternalSdksStorageDir, platformName);
      var sdkPath = Path.Combine(sdkDir, blobName);
      var actualChecksum = this.GetSHA512Checksum(sdkPath);
      if (expectedChecksum == actualChecksum)
      {
          this.logger.LogInformation("Downloaded SDK checksum verified successfully for platform {platformName}, blobName {blobName}", platformName, blobName);
          return true;
      }
      else
      {
          this.logger.LogError("Checksum verification failed for downloaded SDK: platform {platformName}, blobName {blobName}", platformName, blobName);
          return false;
      }
    }

    private async Task<bool> SendRequestAsync(SdkProviderRequest request)
    {
      this.logger.LogDebug("Sending request to external SDK provider: {request}", request);

      using var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

      try
      {
        await socket.ConnectAsync(new UnixDomainSocketEndPoint(SocketPath));

        var requestJson = JsonSerializer.Serialize(request);

        // append $ at the end of the string to indicate end of request
        requestJson += "$";
        var requestBytes = Encoding.UTF8.GetBytes(requestJson);

        // Send the request
        await socket.SendAsync(new ArraySegment<byte>(requestBytes), SocketFlags.None);

        // Receive the response
        var buffer = new byte[4096];
        var received = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);

        var responseString = Encoding.UTF8.GetString(buffer, 0, received);
        if (!string.IsNullOrEmpty(responseString) && responseString.EqualsIgnoreCase("Success"))
        {
          return true;
        }

        return false;
      }
      catch (Exception ex)
      {
        this.logger.LogError(ex, "Error communicating with external SDK provider socket.");
        throw;
      }
    }

    private string GetSHA512Checksum(string filePath)
    {
      using var sha512 = SHA512.Create();
      using var stream = File.OpenRead(filePath);
      var hash = sha512.ComputeHash(stream);
      return BitConverter.ToString(hash).Replace("-", string.Empty);
    }

    private class SdkProviderRequest
    {
      public string PlatformName { get; set; }

      public string BlobName { get; set; }

      public IDictionary<string, string> UrlParameters { get; set; }
    }
  }
}
