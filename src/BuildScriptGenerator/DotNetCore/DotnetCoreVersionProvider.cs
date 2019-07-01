// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    internal class DotNetCoreVersionProvider : IDotNetCoreVersionProvider
    {
        private readonly DotNetCoreScriptGeneratorOptions _options;
        private IEnumerable<string> _supportedVersions;

        public DotNetCoreVersionProvider(IOptions<DotNetCoreScriptGeneratorOptions> options)
        {
            _options = options.Value;
        }

        public IEnumerable<string> SupportedDotNetCoreVersions
        {
            get
            {
                if (_supportedVersions == null)
                {
                    _supportedVersions = VersionProviderHelpers.GetSupportedVersions(
                        _options.SupportedVersions,
                        _options.InstalledVersionsDir);
                }

                return _supportedVersions;
            }
        }
    }
}