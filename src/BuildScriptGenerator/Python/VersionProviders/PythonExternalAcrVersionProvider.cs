// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            ILoggerFactory loggerFactory)
            : base(commonOptions, loggerFactory)
        {
        }

        public virtual PlatformVersionInfo GetVersionInfo()
        {
            return this.GetAvailableVersionsFromExternalAcr(platformName: ToolNameConstants.PythonName);
        }
    }
}
