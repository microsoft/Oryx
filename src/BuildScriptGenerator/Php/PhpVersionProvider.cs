// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    internal class PhpVersionProvider : IPhpVersionProvider
    {
        private readonly PhpScriptGeneratorOptions _opts;
        private IEnumerable<string> _supportedPhpVersions;

        public PhpVersionProvider(IOptions<PhpScriptGeneratorOptions> options)
        {
            _opts = options.Value;
        }

        public IEnumerable<string> SupportedPhpVersions
        {
            get
            {
                if (_supportedPhpVersions == null)
                {
                    _supportedPhpVersions = VersionProviderHelper.GetSupportedVersions(
                        _opts.SupportedPhpVersions,
                        _opts.InstalledPhpVersionsDir);
                }

                return _supportedPhpVersions;
            }
        }
    }
}