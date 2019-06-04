// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    internal class DotNetCoreScriptGeneratorOptionsSetup : IConfigureOptions<DotNetCoreScriptGeneratorOptions>
    {
        internal const string DefaultVersion = DotNetCoreRuntimeVersions.NetCoreApp21;
        internal const string InstalledVersionsDir = "/opt/dotnet/runtimes";

        private readonly IEnvironment _environment;

        public DotNetCoreScriptGeneratorOptionsSetup(IEnvironment environment)
        {
            _environment = environment;
        }

        public void Configure(DotNetCoreScriptGeneratorOptions options)
        {
            var defaultVersion = _environment.GetEnvironmentVariable(EnvironmentSettingsKeys.DotNetCoreDefaultVersion);
            if (string.IsNullOrEmpty(defaultVersion))
            {
                defaultVersion = DefaultVersion;
            }

            options.DefaultVersion = defaultVersion;
            options.InstalledVersionsDir = InstalledVersionsDir;
            options.SupportedVersions = _environment.GetEnvironmentVariableAsList(
                EnvironmentSettingsKeys.DotNetCoreSupportedVersions);
            options.Project = _environment.GetEnvironmentVariable(EnvironmentSettingsKeys.Project);
            options.MSBuildConfiguration = _environment.GetEnvironmentVariable(
                EnvironmentSettingsKeys.MSBuildConfiguration);
        }
    }
}