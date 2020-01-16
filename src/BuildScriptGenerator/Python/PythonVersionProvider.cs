// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Common;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    internal class PythonVersionProvider : IPythonVersionProvider
    {
        private readonly PythonScriptGeneratorOptions _options;
        private readonly IEnvironment _environment;
        private readonly PythonPlatformInstaller _platformInstaller;
        private IEnumerable<string> _supportedPythonVersions;

        public PythonVersionProvider(
            IOptions<PythonScriptGeneratorOptions> options,
            IEnvironment environment,
            PythonPlatformInstaller platformInstaller)
        {
            _options = options.Value;
            _environment = environment;
            _platformInstaller = platformInstaller;
        }

        public IEnumerable<string> SupportedPythonVersions
        {
            get
            {
                if (_supportedPythonVersions == null)
                {
                    var useLatestVersion = _environment.GetBoolEnvironmentVariable(
                        SdkStorageConstants.UseLatestVersion);
                    if (useLatestVersion.HasValue && useLatestVersion.Value)
                    {
                        _supportedPythonVersions = _platformInstaller.GetAvailableVersionsInStorage();
                    }
                    else
                    {
                        _supportedPythonVersions = VersionProviderHelper.GetSupportedVersions(
                            _options.SupportedPythonVersions,
                            _options.InstalledPythonVersionsDir);
                    }
                }

                return _supportedPythonVersions;
            }
        }
    }
}