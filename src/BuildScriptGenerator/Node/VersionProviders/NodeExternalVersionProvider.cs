// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
  internal class NodeExternalVersionProvider : ExternalSdkStorageVersionProviderBase, INodeVersionProvider
  {
    private PlatformVersionInfo platformVersionInfo;

    public NodeExternalVersionProvider(
        IOptions<BuildScriptGeneratorOptions> commonOptions,
        IExternalSdkProvider externalSdkProvider,
        ILoggerFactory loggerFactory)
        : base(commonOptions, externalSdkProvider, loggerFactory)
    {
    }

    // To enable unit testing
    public virtual PlatformVersionInfo GetVersionInfo()
    {
      if (this.platformVersionInfo == null)
      {
        this.platformVersionInfo = this.GetAvailableVersionsFromExternalProvider(platformName: "nodejs");
      }

      return this.platformVersionInfo;
    }
  }
}