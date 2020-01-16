// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Python;
using Microsoft.Oryx.Common;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    internal class PhpVersionProvider : IPhpVersionProvider
    {
        private readonly PhpScriptGeneratorOptions _opts;
        private readonly IEnvironment _environment;
        private readonly PhpPlatformInstaller _platformInstaller;
        private IEnumerable<string> _supportedPhpVersions;

        public PhpVersionProvider(
            IOptions<PhpScriptGeneratorOptions> options,
            IEnvironment environment,
            PhpPlatformInstaller platformInstaller)
        {
            _opts = options.Value;
            _environment = environment;
            _platformInstaller = platformInstaller;
        }

        public IEnumerable<string> SupportedPhpVersions
        {
            get
            {
                if (_supportedPhpVersions == null)
                {
                    var useLatestVersion = _environment.GetBoolEnvironmentVariable(
                        SdkStorageConstants.UseLatestVersion);
                    if (useLatestVersion.HasValue && useLatestVersion.Value)
                    {
                        _supportedPhpVersions = _platformInstaller.GetAvailableVersionsInStorage();
                    }
                    else
                    {
                        _supportedPhpVersions = VersionProviderHelper.GetSupportedVersions(
                            _opts.SupportedPhpVersions,
                            _opts.InstalledPhpVersionsDir);
                    }
                }

                return _supportedPhpVersions;
            }
        }
    }
}