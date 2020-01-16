// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Common;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    internal class DotNetCoreVersionProvider : IDotNetCoreVersionProvider
    {
        private readonly DotNetCoreScriptGeneratorOptions _options;
        private readonly DotNetCorePlatformInstaller _platformInstaller;
        private readonly IEnvironment _environment;
        private IEnumerable<string> _supportedVersions;

        public DotNetCoreVersionProvider(
            IOptions<DotNetCoreScriptGeneratorOptions> options,
            IEnvironment environment,
            DotNetCorePlatformInstaller platformInstaller)
        {
            _options = options.Value;
            _platformInstaller = platformInstaller;
            _environment = environment;
        }

        public IEnumerable<string> SupportedDotNetCoreVersions
        {
            get
            {
                if (_supportedVersions == null)
                {
                    var useLatestVersion = _environment.GetBoolEnvironmentVariable(
                        SdkStorageConstants.UseLatestVersion);
                    if (useLatestVersion.HasValue && useLatestVersion.Value)
                    {
                        _supportedVersions = _platformInstaller.GetAvailableVersionsInStorage();
                    }
                    else
                    {
                        _supportedVersions = VersionProviderHelper.GetSupportedVersions(
                            _options.SupportedVersions,
                            _options.InstalledVersionsDir);
                    }
                }

                return _supportedVersions;
            }
        }
    }
}