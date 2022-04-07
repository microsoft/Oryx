// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    public class NodePlatformInstaller : PlatformInstallerBase
    {
        public NodePlatformInstaller(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            ILoggerFactory loggerFactory)
            : base(commonOptions, loggerFactory)
        {
        }

        public virtual string GetInstallerScriptSnippet(string version)
        {
            return this.GetInstallerScriptSnippet(NodeConstants.PlatformName, version);
        }

        public virtual bool IsVersionAlreadyInstalled(string version)
        {
            return this.IsVersionInstalled(
                version,
                builtInDir: NodeConstants.InstalledNodeVersionsDir,
                dynamicInstallDir: Path.Combine(this.CommonOptions.DynamicInstallRootDir, NodeConstants.PlatformName));
        }
    }
}
