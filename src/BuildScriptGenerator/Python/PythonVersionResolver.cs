// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Logging;
using SemVer;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    internal class PythonVersionResolver : IPythonVersionResolver
    {
        private readonly IPythonVersionProvider _pythonVersionProvider;
        private readonly ILogger<PythonVersionResolver> _logger;

        public PythonVersionResolver(
            IPythonVersionProvider pythonVersionProvider,
            ILogger<PythonVersionResolver> logger)
        {
            _pythonVersionProvider = pythonVersionProvider;
            _logger = logger;
        }

        /// <summary>
        /// <see cref="IPythonVersionProvider.GetSupportedPythonVersion(string)"/>
        /// </summary>
        public string GetSupportedPythonVersion(string versionRange)
        {
            try
            {
                var range = new Range(versionRange);
                var satisfying = range.MaxSatisfying(_pythonVersionProvider.SupportedPythonVersions);
                return satisfying;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "An error occurred while trying to find supported Python version.");
            }

            return null;
        }
    }
}
