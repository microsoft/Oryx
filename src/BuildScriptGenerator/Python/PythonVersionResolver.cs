// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using SemVer;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    internal class PythonVersionResolver : IPythonVersionResolver
    {
        private readonly IPythonVersionProvider _pythonVersionProvider;

        public PythonVersionResolver(IPythonVersionProvider pythonVersionProvider)
        {
            _pythonVersionProvider = pythonVersionProvider;
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
            catch
            {
                return null;
            }
        }
    }
}
