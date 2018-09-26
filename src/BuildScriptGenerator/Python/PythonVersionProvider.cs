// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                    _supportedPythonVersions = GetVersionsFromDirectory(_options.InstalledPythonVersionsDir);
                }
                return _supportedPythonVersions;
            }
        }

        private static IEnumerable<string> GetVersionsFromDirectory(string versionsDir)
        {
            var listOptions = new EnumerationOptions()
            {
                RecurseSubdirectories = false,
                IgnoreInaccessible = false,
            };

            var versionDirectories = Directory.EnumerateDirectories(versionsDir, "*", listOptions)
                .Select(versionDir => new DirectoryInfo(versionDir));

            var versions = new List<SemVer.Version>();
            foreach (var versionDir in versionDirectories)
            {
                try
                {
                    var version = new SemVer.Version(versionDir.Name);
                    versions.Add(version);
                }
                catch (ArgumentException)
                {
                    // ignore non semantic version based versions like 'latest' or 'lts'
                }
            }
            versions.Sort();

            return versions.Select(v => v.ToString());
        }
    }
}