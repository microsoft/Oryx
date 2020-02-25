// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    public class DotNetCorePlatformInstaller : PlatformInstallerBase
    {
        private readonly IDotNetCoreVersionProvider _versionProvider;

        public DotNetCorePlatformInstaller(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IEnvironment environment,
            IDotNetCoreVersionProvider versionProvider)
            : base(commonOptions, environment)
        {
            _versionProvider = versionProvider;
        }

        public override string GetInstallerScriptSnippet(string runtimeVersion)
        {
            var versionMap = _versionProvider.GetSupportedVersions();
            var sdkVersion = versionMap[runtimeVersion];
            var dirToInstall =
                $"{Constants.TemporaryInstallationDirectoryRoot}/{DotNetCoreConstants.LanguageName}/sdks/{sdkVersion}";
            var sdkInstallerScript = GetInstallerScriptSnippet(
                DotNetCoreConstants.LanguageName,
                sdkVersion,
                dirToInstall);
            var dotnetDir = $"{Constants.TemporaryInstallationDirectoryRoot}/{DotNetCoreConstants.LanguageName}";

            // Create the following structure so that 'benv' tool can understand it as it already does.
            var scriptBuilder = new StringBuilder();
            scriptBuilder
            .AppendLine(sdkInstallerScript)
            .AppendLine($"mkdir -p {dotnetDir}/runtimes/{runtimeVersion}")
            .AppendLine($"echo '{sdkVersion}' > {dotnetDir}/runtimes/{runtimeVersion}/sdkVersion.txt");
            return scriptBuilder.ToString();
        }

        public override bool IsVersionAlreadyInstalled(string version)
        {
            return IsVersionInstalled(
                version,
                installationDirs: new[]
                {
                    DotNetCoreConstants.InstalledDotNetCoreRuntimeVersionsDir,
                    $"{Constants.TemporaryInstallationDirectoryRoot}/dotnet/runtimes"
                });
        }
    }
}
