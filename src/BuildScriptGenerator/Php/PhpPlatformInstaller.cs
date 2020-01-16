// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Php;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    public class PhpPlatformInstaller : PlatformInstallerBase
    {
        public PhpPlatformInstaller(
            IOptions<BuildScriptGeneratorOptions> commonOptions, 
            IEnvironment environment,
            IHttpClientFactory httpClientFactory)
            : base(commonOptions, environment, httpClientFactory)
        {
        }

        public override string GetInstallerScriptSnippetForBuildImage(string version)
        {
            return GetInstallerScriptSnippetForBuildImage(PhpConstants.PhpName, version);
        }

        public override string GetInstallerScriptSnippetForDeveloperImage(string version)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<string> GetAvailableVersionsForBuildImage()
        {
            return GetAvailableVersionsInBuildImage(PhpConstants.PhpName);
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
                    "/opt/php",
                    $"{Constants.TempInstallationDirRoot}/php"
                });
        }

        public override bool IsVersionInstalledInDeveloperImage(string version)
        {
            throw new NotImplementedException();
        }
    }
}
