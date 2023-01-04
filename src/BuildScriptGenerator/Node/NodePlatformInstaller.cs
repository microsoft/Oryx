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
            _ = stringBuilder.AppendLine($"echo 'Installing {NodeConstants.PlatformName} specific dependencies...'");

            // Install Hugo and Yarn for node applications
            _ = stringBuilder.AppendLine("BUILD_DIR=\"/opt/tmp/build\"");
            _ = stringBuilder.AppendLine("IMAGES_DIR=\"/opt/tmp/images\"");
            _ = stringBuilder.AppendLine("${IMAGES_DIR}/build/installHugo.sh");
            _ = stringBuilder.AppendLine("set -ex");
            _ = stringBuilder.AppendLine("yarnCacheFolder=\"/usr/local/share/yarn-cache\"");
            _ = stringBuilder.AppendLine("mkdir -p $yarnCacheFolder");
            _ = stringBuilder.AppendLine("chmod 777 $yarnCacheFolder");
            _ = stringBuilder.AppendLine(". ${BUILD_DIR}/__nodeVersions.sh");
            _ = stringBuilder.AppendLine("${IMAGES_DIR}/receiveGpgKeys.sh 6A010C5166006599AA17F08146C2130DFD2497F5");
            _ = stringBuilder.AppendLine("${IMAGES_DIR}/retry.sh \"curl -fsSLO --compressed https://yarnpkg.com/downloads/$YARN_VERSION/yarn-v$YARN_VERSION.tar.gz\"");
            _ = stringBuilder.AppendLine("${IMAGES_DIR}/retry.sh \"curl -fsSLO --compressed https://yarnpkg.com/downloads/$YARN_VERSION/yarn-v$YARN_VERSION.tar.gz.asc\"");
            _ = stringBuilder.AppendLine("gpg --batch --verify yarn-v$YARN_VERSION.tar.gz.asc yarn-v$YARN_VERSION.tar.gz");
            _ = stringBuilder.AppendLine("mkdir -p /opt/yarn");
            _ = stringBuilder.AppendLine("tar -xzf yarn-v$YARN_VERSION.tar.gz -C /opt/yarn");
            _ = stringBuilder.AppendLine("mv /opt/yarn/yarn-v$YARN_VERSION /opt/yarn/$YARN_VERSION");
            _ = stringBuilder.AppendLine("rm yarn-v$YARN_VERSION.tar.gz.asc yarn-v$YARN_VERSION.tar.gz");
            _ = stringBuilder.AppendLine(". ${BUILD_DIR}/__nodeVersions.sh");
            _ = stringBuilder.AppendLine("ln -s $YARN_VERSION /opt/yarn/stable");
            _ = stringBuilder.AppendLine("ln -s $YARN_VERSION /opt/yarn/latest");
            _ = stringBuilder.AppendLine("ln -s $YARN_VERSION /opt/yarn/$YARN_MINOR_VERSION");
            _ = stringBuilder.AppendLine("ln -s $YARN_MINOR_VERSION /opt/yarn/$YARN_MAJOR_VERSION");
            _ = stringBuilder.AppendLine("mkdir -p /links");
            _ = stringBuilder.AppendLine("cp -s /opt/yarn/stable/bin/yarn /opt/yarn/stable/bin/yarnpkg /links");

            InstallPythonToolingAndLanguage(stringBuilder);
        }
    }
}
