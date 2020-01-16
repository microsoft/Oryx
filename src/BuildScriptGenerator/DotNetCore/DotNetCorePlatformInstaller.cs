// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    public class DotNetCorePlatformInstaller : PlatformInstallerBase
    {
        public DotNetCorePlatformInstaller(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IEnvironment environment,
            IHttpClientFactory httpClientFactory)
            : base(commonOptions, environment, httpClientFactory)
        { }

        public Dictionary<string, string> RuntimeAndSdkVersions { get; private set; }

        public override string GetInstallerScriptSnippetForBuildImage(string runtimeVersion)
        {
            GetAvailableVersionsForBuildImage();
            var dotnetDir = $"{Constants.TempInstallationDirRoot}/{DotNetCoreConstants.LanguageName}";
            var sdkVersion = RuntimeAndSdkVersions[runtimeVersion];
            var installSdkScript = GetInstallerScriptSnippetForBuildImage(
                DotNetCoreConstants.LanguageName,
                sdkVersion,
                directoryToInstall: $"{dotnetDir}/sdks/{sdkVersion}");
            var sb = new StringBuilder();
            sb
            .AppendLine(installSdkScript)
            .AppendLine($"mkdir -p {dotnetDir}/runtimes/{runtimeVersion}")
            .AppendLine($"ln -s {dotnetDir}/sdks/{sdkVersion} {dotnetDir}/runtimes/{runtimeVersion}/sdk");
            return sb.ToString();
        }

        public override IEnumerable<string> GetAvailableVersionsForBuildImage()
        {
            if (RuntimeAndSdkVersions == null)
            {
                // Each version is a pair of runtime and sdk version
                var versions = GetAvailableVersionsInBuildImage(DotNetCoreConstants.LanguageName);
                var runtimeAndSdkVersions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var version in versions)
                {
                    var parts = version.Split(',');
                    if (parts.Length == 2)
                    {
                        runtimeAndSdkVersions[parts[0].Trim()] = parts[1].Trim();
                    }
                }

                RuntimeAndSdkVersions = runtimeAndSdkVersions;
            }

            return RuntimeAndSdkVersions.Keys;
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
                    "/opt/dotnet/sdks",
                    $"{Constants.TempInstallationDirRoot}/dotnet"
                });
        }

        public override string GetInstallerScriptSnippetForDeveloperImage(string version)
        {
            throw new NotImplementedException();
        }

        public override bool IsVersionInstalledInDeveloperImage(string version)
        {
            throw new NotImplementedException();
        }
    }
}
