// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    /// <summary>
    /// ACR-based version provider for Python SDKs via external socket provider.
    /// Parallel to <see cref="PythonExternalVersionProvider"/> (blob) and
    /// <see cref="PythonAcrVersionProvider"/> (direct OCI).
    /// </summary>
    internal class PythonExternalAcrVersionProvider : ExternalAcrVersionProviderBase, IPythonVersionProvider
    {
        public PythonExternalAcrVersionProvider(
            ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
        }

        public virtual PlatformVersionInfo GetVersionInfo()
        {
            var version = this.GetCompanionSdkVersion(platformName: ToolNameConstants.PythonName);
            if (string.IsNullOrEmpty(version))
            {
                return null;
            }

            return PlatformVersionInfo.CreateOnDiskVersionInfo(
                supportedVersions: new[] { version },
                defaultVersion: version);
        }
    }
}
