// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Common;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    internal class NodeVersionProvider : INodeVersionProvider
    {
        private readonly NodeScriptGeneratorOptions _options;
        private readonly NodePlatformInstaller _platformInstaller;
        private readonly IEnvironment _environment;
        private IEnumerable<string> _supportedNodeVersions;
        private IEnumerable<string> _supportedNpmVersions;

        public NodeVersionProvider(
            IOptions<NodeScriptGeneratorOptions> options,
            IEnvironment environment,
            NodePlatformInstaller platformInstaller)
        {
            _options = options.Value;
            _platformInstaller = platformInstaller;
            _environment = environment;
        }

        public IEnumerable<string> SupportedNodeVersions
        {
            get
            {
                if (_supportedNodeVersions == null)
                {
                    var useLatestVersion = _environment.GetBoolEnvironmentVariable(
                        SdkStorageConstants.UseLatestVersion);
                    if (useLatestVersion.HasValue && useLatestVersion.Value)
                    {
                        _supportedNodeVersions = _platformInstaller.GetAvailableVersionsInStorage();
                    }
                    else
                    {
                        _supportedNodeVersions = VersionProviderHelper.GetSupportedVersions(
                            _options.SupportedNodeVersions,
                            _options.InstalledNodeVersionsDir);
                    }
                }

                return _supportedNodeVersions;
            }
        }

        public IEnumerable<string> SupportedNpmVersions
        {
            get
            {
                if (_supportedNpmVersions == null)
                {
                    _supportedNpmVersions = VersionProviderHelper.GetSupportedVersions(
                        _options.SupportedNpmVersions,
                        _options.InstalledNpmVersionsDir);
                }

                return _supportedNpmVersions;
            }
        }
    }
}