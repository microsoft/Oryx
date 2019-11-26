// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    internal class PhpVersionProvider : IPhpVersionProvider
    {
        public static readonly string[] SupportedVersions = new[] { ">=5 <8" };

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
                    _supportedPhpVersions = _opts.SupportedPhpVersions != null
                        ? _opts.SupportedPhpVersions : SupportedVersions;
                }

                return _supportedPhpVersions;
            }
        }
    }
}