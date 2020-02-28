// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    public class NodePlatformInstaller : PlatformInstallerBase
    {
        public NodePlatformInstaller(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IEnvironment environment)
            : base(commonOptions, environment)
        {
        }

        public override string GetInstallerScriptSnippet(string version)
        {
            return GetInstallerScriptSnippet(NodeConstants.NodeJsName, version);
        }

        public override bool IsVersionAlreadyInstalled(string version)
        {
            return IsVersionInstalled(
                version,
                installationDirs: new[]
                {
                    NodeConstants.InstalledNodeVersionsDir,
                    $"{Constants.TemporaryInstallationDirectoryRoot}/nodejs"
                });
        }
    }
}
