// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    internal class PythonVersionProvider : IPythonVersionProvider
    {
        private IEnumerable<string> _supportedPythonVersions;
        private readonly PythonScriptGeneratorOptions _options;

        public PythonVersionProvider(IOptions<PythonScriptGeneratorOptions> options)
        {
            _options = options.Value;
        }

        public IEnumerable<string> SupportedPythonVersions
        {
            get
            {
                if (_supportedPythonVersions == null)
                {
                    _supportedPythonVersions = VersionProviderHelpers.GetVersionsFromDirectory(_options.InstalledPythonVersionsDir);
                }
                return _supportedPythonVersions;
            }
        }
    }
}