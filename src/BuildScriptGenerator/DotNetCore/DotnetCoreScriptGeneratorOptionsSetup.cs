// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    internal class DotnetCoreScriptGeneratorOptionsSetup : IConfigureOptions<DotnetCoreScriptGeneratorOptions>
    {
        internal const string DefaultVersion = DotNetCoreVersions.DotNetCore21Version;
        internal const string InstalledVersionsDir = "/opt/dotnet/"; // TODO: remove hard-coded path

        private readonly IEnvironment _environment;

        public DotnetCoreScriptGeneratorOptionsSetup(IEnvironment environment)
        {
            _environment = environment;
        }

        public void Configure(DotnetCoreScriptGeneratorOptions options)
        {
            var defaultVersion = _environment.GetEnvironmentVariable(EnvironmentSettingsKeys.DotnetCoreDefaultVersion);
            if (string.IsNullOrEmpty(defaultVersion))
            {
                defaultVersion = DefaultVersion;
            }

            options.DefaultVersion = defaultVersion;
            options.InstalledVersionsDir = InstalledVersionsDir;
            options.SupportedVersions = _environment.GetEnvironmentVariableAsList(
                EnvironmentSettingsKeys.DotnetCoreSupportedVersions);
            options.Project = _environment.GetEnvironmentVariable(EnvironmentSettingsKeys.Project);
            options.MSBuildConfiguration = _environment.GetEnvironmentVariable(
                EnvironmentSettingsKeys.MSBuildConfiguration);
        }
    }
}