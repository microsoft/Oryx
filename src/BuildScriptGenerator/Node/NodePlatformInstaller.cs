// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using System.Text;
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

        public virtual string GetInstallerScriptSnippet(string version, bool skipSdkBinaryDownload = false)
        {
            return this.GetInstallerScriptSnippet(NodeConstants.PlatformName, version, skipSdkBinaryDownload: skipSdkBinaryDownload);
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
            stringBuilder.AppendLine("if grep -q cli \"/opt/oryx/.imagetype\"; then");
            stringBuilder.AppendLine($"echo 'Installing {NodeConstants.PlatformName} specific dependencies...'");

            // Install Hugo and Yarn for node applications
            stringBuilder.AppendLine("BUILD_DIR=\"/opt/tmp/build\"");
            stringBuilder.AppendLine("IMAGES_DIR=\"/opt/tmp/images\"");
            stringBuilder.AppendLine("${IMAGES_DIR}/build/installHugo.sh");
            stringBuilder.AppendLine("set -ex");
            stringBuilder.AppendLine("yarnCacheFolder=\"/usr/local/share/yarn-cache\"");
            stringBuilder.AppendLine("mkdir -p $yarnCacheFolder");
            stringBuilder.AppendLine("chmod 777 $yarnCacheFolder");
            stringBuilder.AppendLine(". ${BUILD_DIR}/__nodeVersions.sh");
            stringBuilder.AppendLine("${IMAGES_DIR}/receiveGpgKeys.sh 6A010C5166006599AA17F08146C2130DFD2497F5");
            stringBuilder.AppendLine("${IMAGES_DIR}/retry.sh \"curl -fsSLO --compressed https://yarnpkg.com/downloads/$YARN_VERSION/yarn-v$YARN_VERSION.tar.gz\"");
            stringBuilder.AppendLine("${IMAGES_DIR}/retry.sh \"curl -fsSLO --compressed https://yarnpkg.com/downloads/$YARN_VERSION/yarn-v$YARN_VERSION.tar.gz.asc\"");
            stringBuilder.AppendLine("gpg --batch --verify yarn-v$YARN_VERSION.tar.gz.asc yarn-v$YARN_VERSION.tar.gz");
            stringBuilder.AppendLine("mkdir -p /opt/yarn");
            stringBuilder.AppendLine("tar -xzf yarn-v$YARN_VERSION.tar.gz -C /opt/yarn");
            stringBuilder.AppendLine("mv /opt/yarn/yarn-v$YARN_VERSION /opt/yarn/$YARN_VERSION");
            stringBuilder.AppendLine("rm yarn-v$YARN_VERSION.tar.gz.asc yarn-v$YARN_VERSION.tar.gz");
            stringBuilder.AppendLine(". ${BUILD_DIR}/__nodeVersions.sh");
            stringBuilder.AppendLine("ln -s $YARN_VERSION /opt/yarn/stable");
            stringBuilder.AppendLine("ln -s $YARN_VERSION /opt/yarn/latest");
            stringBuilder.AppendLine("ln -s $YARN_VERSION /opt/yarn/$YARN_MINOR_VERSION");
            stringBuilder.AppendLine("ln -s $YARN_MINOR_VERSION /opt/yarn/$YARN_MAJOR_VERSION");
            stringBuilder.AppendLine("mkdir -p /links");
            stringBuilder.AppendLine("cp -s /opt/yarn/stable/bin/yarn /opt/yarn/stable/bin/yarnpkg /links");

            InstallPythonToolingAndLanguage(stringBuilder);
            stringBuilder.AppendLine("fi");
        }
    }
}
