// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    public class PythonPlatformInstaller : PlatformInstallerBase
    {
        public PythonPlatformInstaller(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IEnvironment environment,
            IHttpClientFactory httpClientFactory)
            : base(commonOptions, environment, httpClientFactory)
        {
        }

        public override string GetInstallerScriptSnippetForBuildImage(string version)
        {
            return GetInstallerScriptSnippetForBuildImage(PythonConstants.PythonName, version);
        }

        public override string GetInstallerScriptSnippetForDeveloperImage(string version)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<string> GetAvailableVersionsForBuildImage()
        {
            return GetAvailableVersionsInBuildImage(PythonConstants.PythonName);
        }

        public override IEnumerable<string> GetAvailableVersionsForDeveloperImage()
        {
            throw new NotImplementedException();
        }

        public override bool IsVersionInstalledInBuildImage(string version)
        {
            return IsVersionInstalledInBuildImage(
                version,
                installationDirs: new[]
                {
                    "/opt/python",
                    $"{Constants.TempInstallationDirRoot}/python"
                });
        }

        public override bool IsVersionInstalledInDeveloperImage(string version)
        {
            throw new NotImplementedException();
        }
    }
}
