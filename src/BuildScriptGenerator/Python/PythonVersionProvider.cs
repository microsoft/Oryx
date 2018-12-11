// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    internal class PythonVersionProvider : IPythonVersionProvider
    {
        private readonly PythonScriptGeneratorOptions _options;
        private IEnumerable<string> _supportedPythonVersions;

        public PythonVersionProvider(IOptions<PythonScriptGeneratorOptions> options)
        {
            _options = options.Value;
        }

        public IEnumerable<string> SupportedPythonVersions
        {
            get
            {
                if (_supportedPythonVersions == null &&
                    _options.SupportedPythonVersions != null &&
                    _options.SupportedPythonVersions.Any())
                {
                    _supportedPythonVersions = _options.SupportedPythonVersions;
                }

                if (_supportedPythonVersions == null)
                {
                    _supportedPythonVersions = VersionProviderHelpers.GetVersionsFromDirectory(_options.InstalledPythonVersionsDir);
                }

                return _supportedPythonVersions;
            }
        }
    }
}