// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.DotnetCore
{
    internal class DotnetCoreVersionProvider : IDotnetCoreVersionProvider
    {
        private readonly DotnetCoreScriptGeneratorOptions _options;
        private IEnumerable<string> _supportedVersions;

        public DotnetCoreVersionProvider(IOptions<DotnetCoreScriptGeneratorOptions> options)
        {
            _options = options.Value;
        }

        public IEnumerable<string> SupportedDotNetCoreVersions
        {
            get
            {
                if (_supportedVersions == null)
                {
                    _supportedVersions = _options.SupportedVersions;
                    if (_supportedVersions == null)
                    {
                        _supportedVersions = VersionProviderHelpers.GetVersionsFromDirectory(
                            _options.InstalledVersionsDir);
                    }
                }

                return _supportedVersions;
            }
        }
    }
}