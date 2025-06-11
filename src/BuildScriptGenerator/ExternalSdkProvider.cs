// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGenerator
{
  /// <summary>
  /// Provides SDK download functionality by communicating with an external service via Unix domain socket.
  /// </summary>
  public class ExternalSdkProvider : IExternalSdkProvider
  {
    public const string ExternalSdksStorageDir = "/var/OryxSdks";
    private const string SocketPath = "/var/sockets/oryx-pull-sdk.socket";
    private const int MaxTimeoutForSocketOperationInSeconds = 100;
    private readonly ILogger<ExternalSdkProvider> logger;
    private readonly IStandardOutputWriter outputWriter;

    public ExternalSdkProvider(
    IStandardOutputWriter outputWriter,
    ILogger<ExternalSdkProvider> logger)
    {
      this.logger = logger;
      this.outputWriter = outputWriter;
    }

    /// <inheritdoc />
    public async Task<XDocument> GetPlatformMetaDataAsync(string platformName)
    {
      try
      {
        var request = new SdkProviderRequest
        {
          PlatformName = platformName,
          BlobName = null,
          UrlParameters = new Dictionary<string, string>
                      {
                        { "restype", "container" },
                        { "comp", "list" },
                        { "include", "metadata" },
                      },
        };

        var filePath = Path.Combine(ExternalSdksStorageDir, platformName, platformName);
        this.logger.LogInformation("Requesting metadata for platform {} from external SDK provider, expected filepath: {filePath}", platformName, filePath);
        this.outputWriter.WriteLine($"Requesting metadata for platform {platformName} from external SDK provider");
        var response = await this.SendRequestAsync(request);

        if (response && File.Exists(filePath))
        {
          this.logger.LogInformation("Successfully got metadata for platform {platformName}, available at filePath: {filePath}", platformName, filePath);
          try
          {
            return XDocument.Load(filePath);
          }
          catch (Exception ex)
          {
            throw new InvalidOperationException($"Error loading metadata file {filePath} for platform {platformName}", ex);
          }
        }
        else
        {
          throw new InvalidOperationException($"Failed to get metadata for platform {platformName} from external SDK provider");
        }
      }
      catch (Exception ex)
      {
        this.outputWriter.WriteLine($"Error getting metadata for platform {platformName} from external provider: {ex.Message}");
        throw new InvalidOperationException($"Error getting metadata for platform {platformName} from external provider.", ex);
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
          this.logger.LogError("Unable to get checksum for Platform: {platformName}, Blob: {blobName} as fetching metadata from external provider failed.", platformName, blobName);
          return null;
        }
        else
        {
          var blobElement = xdoc.XPathSelectElement($"//Blobs/Blob[Name='{blobName}']");
          var checksum = blobElement?
            .Element("Metadata")?
            .Elements()
            .FirstOrDefault(e => string.Equals(e.Name.LocalName, "Checksum", StringComparison.OrdinalIgnoreCase))
            ?.Value;
          if (string.IsNullOrEmpty(checksum))
          {
            this.logger.LogError("Checksum not found for platform {platformName}, blobname {blobName} from external provider.", platformName, blobName);
            return null;
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

    public bool CheckLocalCacheForBlob(string platformName, string blobName, string expectedChecksum)
    {
      var blobPath = Path.Combine(ExternalSdksStorageDir, platformName, blobName);
      if (File.Exists(blobPath))
      {
        var actualChecksum = this.GetSHA512Checksum(blobPath);
        if (string.Equals(actualChecksum, expectedChecksum, StringComparison.OrdinalIgnoreCase))
        {
          this.logger.LogInformation("Blob for platform {platformName}, blobName {blobName} already exists in external provider cache at {blobPath} and checksum matches", platformName, blobName, blobPath);
          return true;
        }
        else
        {
          this.logger.LogWarning("Blob for platform {platformName}, blobName {blobName} already exists in external provider cache at {blobPath} but checksum does not match, considering the blob as corrupted", platformName, blobName, blobPath);
        }
      }

      return false;
    }

    /// <inheritdoc />
    public async Task<bool> RequestBlobAsync(string platformName, string blobName)
    {
      // Get the expected checksum for the SDK
      var expectedChecksum = await this.GetChecksumForVersionAsync(platformName, blobName);
      if (expectedChecksum == null)
      {
        this.outputWriter.WriteLine($"Failed to get checksum for platform {platformName}, blobName {blobName}. Skipping download of blob.");
        this.logger.LogError("Failed to get checksum for blob : platform {platformName}, blobName {blobName}. Skipping download of blob.", platformName, blobName);
        return false;
      }

      if (this.CheckLocalCacheForBlob(platformName, blobName, expectedChecksum))
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
            UrlParameters = new Dictionary<string, string>(),
          };

          var response = await this.SendRequestAsync(request);
          var filePath = Path.Combine(ExternalSdksStorageDir, platformName, blobName);

          if (response && File.Exists(filePath))
          {
            this.outputWriter.WriteLine($"Successfully requested blob for platform {platformName}, blobName {blobName}, available at {filePath}");
            this.logger.LogInformation("Successfully requested blob for platform {platformName}, blobName {blobName}, available at {FilePath}", platformName, blobName, filePath);
          }
          else
          {
            this.outputWriter.WriteLine($"Failed to get blob for platform {platformName}, blobName {blobName} from external provider.");
            this.logger.LogError("Failed to get blob for platform {platformName}, blobName {blobName}", platformName, blobName);
            return false;
          }
        }
        catch (Exception ex)
        {
          this.outputWriter.WriteLine($"Failed to get blob for platform {platformName}, blobName {blobName} from external provider. Exception : {ex}");
          this.logger.LogError(ex, "Error requesting blob for platform {platformName} blobName {blobName} from external provider.", platformName, blobName);
          return false;
        }
      }

      // Verify checksum
      var blobPath = Path.Combine(ExternalSdksStorageDir, platformName, blobName);
      var actualChecksum = this.GetSHA512Checksum(blobPath);
      if (string.Equals(actualChecksum, expectedChecksum, StringComparison.OrdinalIgnoreCase))
      {
        this.logger.LogInformation("Downloaded blob checksum verified successfully for platform {platformName}, blobName {blobName}", platformName, blobName);
        return true;
      }
      else
      {
        this.logger.LogError("Checksum verification failed for downloaded blob: platform {platformName}, blobName {blobName}, actual checksum {actualChecksum} , expected checksum {expectedChecksum}", platformName, blobName, actualChecksum, expectedChecksum);
        return false;
      }
    }

    private async Task<bool> SendRequestAsync(SdkProviderRequest request)
    {
      using var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
      try
      {
        this.logger.LogInformation("Sending request to external SDK provider: {PlatformName} , {BlobName}, UrlParameters: {UrlParamsJson}", request.PlatformName, request.BlobName, JsonSerializer.Serialize(request.UrlParameters));

        using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(MaxTimeoutForSocketOperationInSeconds)))
        {
          await socket.ConnectAsync(new UnixDomainSocketEndPoint(SocketPath), cts.Token);
          var requestJson = JsonSerializer.Serialize(request);
          this.logger.LogInformation("Connected to socket {socketPath} and sending request: {requestJson}", SocketPath, requestJson);

          // append $ at the end of the string to indicate end of request
          requestJson += "$";
          var requestBytes = Encoding.UTF8.GetBytes(requestJson);

          await socket.SendAsync(new ArraySegment<byte>(requestBytes), SocketFlags.None, cts.Token);
          var buffer = new byte[4096];
          var received = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None, cts.Token);
          var responseString = Encoding.UTF8.GetString(buffer, 0, received);
          this.logger.LogInformation("Received response from external SDK provider: {response}", responseString);
          if (!string.IsNullOrEmpty(responseString) && responseString.EqualsIgnoreCase("Success$"))
          {
            return true;
          }
          else
          {
            this.logger.LogError("Request to external SDK provider was unsuccessful. Response: {response}", responseString);
          }
        }
      }
      catch (OperationCanceledException)
      {
        this.outputWriter.WriteLine("The external SDK provider operation was canceled due to timeout.");
        this.logger.LogError("The external SDK provider operation was canceled due to timeout.");
      }
      catch (Exception ex)
      {
        this.outputWriter.WriteLine($"Error communicating with external SDK provider: {ex.Message}");
        this.logger.LogError(ex, "Error communicating with external SDK provider.");
      }

      return false;
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
