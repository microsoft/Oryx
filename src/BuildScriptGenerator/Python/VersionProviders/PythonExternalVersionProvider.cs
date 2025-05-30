// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
  internal class PythonExternalVersionProvider : ExternalSdkStorageVersionProviderBase, IPythonVersionProvider
  {
    public PythonExternalVersionProvider(
        IOptions<BuildScriptGeneratorOptions> commonOptions,
        IExternalSdkProvider externalSdkProvider,
        ILoggerFactory loggerFactory)
        : base(commonOptions, externalSdkProvider, loggerFactory)
    {
    }

    // To enable unit testing
    public virtual PlatformVersionInfo GetVersionInfo()
    {
      return this.GetAvailableVersionsFromExternalProvider(platformName: ToolNameConstants.PythonName);
    }
  }
}