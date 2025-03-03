// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator
{
  /// <summary>
  /// Provides SDK download functionality by communicating with an external service via Unix domain socket.
  /// </summary>
  public class ExternalSdkProvider : IExternalSdkProvider
  {
    private const string SocketPath = "/var/run/oryx-sdk-provider.sock";
    private const string ExternalSdkStorageDir = "/tmp/sdks";
    private readonly BuildScriptGeneratorOptions _options;
    private readonly ILogger<ExternalSdkProvider> _logger;

    public ExternalSdkProvider(
        IOptions<BuildScriptGeneratorOptions> options,
        ILogger<ExternalSdkProvider> logger)
    {
      _options = options.Value;
      _logger = logger;
    }

    /// <inheritdoc />
    public bool IsEnabled()
    {
      return _options.EnableExternalSdkProvider;
    }

    /// <inheritdoc />
    public async Task<List<string>> GetAvailableVersionsAsync(string platformName)
    {
      if (!IsEnabled())
      {
        _logger.LogDebug("External SDK provider is not enabled.");
        return new List<string>();
      }

      try
      {
        var request = new SdkProviderRequest
        {
          RequestType = "list_versions",
          PlatformName = platformName,
        };

        var response = await SendRequestAsync(request);
        return response.Versions ?? new List<string>();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting available versions for platform {platformName} from external SDK provider.", platformName);
        return new List<string>();
      }
    }

    /// <inheritdoc />
    public async Task<string> RequestSdkAsync(string platformName, string version)
    {
      if (!IsEnabled())
      {
        _logger.LogDebug("External SDK provider is not enabled.");
        return null;
      }

      try
      {
        var request = new SdkProviderRequest
        {
          RequestType = "download_sdk",
          PlatformName = platformName,
          Version = version
        };

        var response = await SendRequestAsync(request);

        if (response.Success && !string.IsNullOrEmpty(response.SdkPath))
        {
          _logger.LogInformation(
              "Successfully requested SDK for platform {platformName} version {version}, available at {sdkPath}",
              platformName, version, response.SdkPath);
          return response.SdkPath;
        }
        else
        {
          _logger.LogError(
              "Failed to get SDK for platform {platformName} version {version}: {errorMessage}",
              platformName, version, response.ErrorMessage);
          return null;
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error requesting SDK for platform {platformName} version {version} from external provider.", platformName, version);
        return null;
      }
    }

    private async Task<SdkProviderResponse> SendRequestAsync(SdkProviderRequest request)
    {
      _logger.LogDebug("Sending request to external SDK provider: {requestType} for platform {platformName}, version {version}",
          request.RequestType, request.PlatformName, request.Version ?? "n/a");

      using var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

      try
      {
        await socket.ConnectAsync(new UnixDomainSocketEndPoint(SocketPath));

        var requestJson = JsonSerializer.Serialize(request);
        var requestBytes = Encoding.UTF8.GetBytes(requestJson);

        // Send the request
        await socket.SendAsync(new ArraySegment<byte>(requestBytes), SocketFlags.None);

        // Receive the response
        var buffer = new byte[4096];
        var received = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);

        var responseJson = Encoding.UTF8.GetString(buffer, 0, received);
        var response = JsonSerializer.Deserialize<SdkProviderResponse>(responseJson);

        return response;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error communicating with external SDK provider socket.");
        throw;
      }
    }

    private class SdkProviderRequest
    {
      public string RequestType { get; set; }
      public string PlatformName { get; set; }
      public string Version { get; set; }
    }

    private class SdkProviderResponse
    {
      public bool Success { get; set; }
      public string SdkPath { get; set; }
      public string ErrorMessage { get; set; }
      public List<string> Versions { get; set; }
    }
  }
}
