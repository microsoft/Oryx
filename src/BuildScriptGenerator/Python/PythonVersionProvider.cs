// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    internal class PythonVersionProvider : IPythonVersionProvider
    {
        public static readonly string[] SupportedVersions = new[] { ">=2.7 <4" };

        private readonly PythonScriptGeneratorOptions _opts;
        private IEnumerable<string> _supportedPythonVersions;

        public PythonVersionProvider(IOptions<PythonScriptGeneratorOptions> options)
        {
            _opts = options.Value;
        }

        public IEnumerable<string> SupportedPythonVersions
        {
            get
            {
                if (_supportedPythonVersions == null)
                {
                    _supportedPythonVersions = _opts.SupportedPythonVersions != null
                        ? _opts.SupportedPythonVersions : SupportedVersions;
                }

                return _supportedPythonVersions;
            }
        }
    }
}