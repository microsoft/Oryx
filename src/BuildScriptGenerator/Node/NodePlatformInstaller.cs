// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;

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

        public override void InstallPlatformSpecificSkeletonDependencies(StringBuilder stringBuilder)
        {
            // Do nothing for image used by builder
        }
    }
}
