// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    public class PythonOnDiskVersionProvider : IPythonVersionProvider
    {
        private readonly PythonScriptGeneratorOptions _options;
        private PlatformVersionInfo _platformVersionInfo;

        public PythonOnDiskVersionProvider(IOptions<PythonScriptGeneratorOptions> options)
        {
            _options = options.Value;
        }

        // To enable unit testing
        public virtual PlatformVersionInfo GetVersionInfo()
        {
            if (_platformVersionInfo == null)
            {
                var installedVersions = VersionProviderHelper.GetVersionsFromDirectory(
                            PythonConstants.InstalledPythonVersionsDir);
                _platformVersionInfo = PlatformVersionInfo.CreateOnDiskVersionInfo(
                    installedVersions,
                    _options.PythonDefaultVersion);
            }

            return _platformVersionInfo;
        }
    }
}
