// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    public class NodePlatformInstaller : PlatformInstallerBase
    {
        public NodePlatformInstaller(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IEnvironment environment,
            IHttpClientFactory httpClientFactory)
            : base(commonOptions, environment, httpClientFactory)
        {
        }

        public override string GetInstallerScriptSnippetForBuildImage(string version)
        {
            return GetInstallerScriptSnippetForBuildImage(NodeConstants.NodeJsName, version);
        }

        public override string GetInstallerScriptSnippetForDeveloperImage(string version)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<string> GetAvailableVersionsForBuildImage()
        {
            return GetAvailableVersionsInBuildImage(NodeConstants.NodeJsName, versionMetadataElementName: "version");
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
                    "/opt/nodejs",
                    $"{Constants.TempInstallationDirRoot}/nodejs"
                });
        }

        public override bool IsVersionInstalledInDeveloperImage(string version)
        {
            throw new NotImplementedException();
        }
    }
}
