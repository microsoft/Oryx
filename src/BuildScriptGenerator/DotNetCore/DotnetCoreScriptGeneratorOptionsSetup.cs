// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    internal class DotnetCoreScriptGeneratorOptionsSetup : IConfigureOptions<DotnetCoreScriptGeneratorOptions>
    {
        internal const string DefaultVersion = DotNetCoreVersions.DotNetCore21Version;

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
            options.InstalledVersionsDir = Path.Combine(
                _environment.GetEnvironmentVariable(EnvironmentSettingsKeys.PlatformsDir, Constants.DefaultPlatformsDir),
                "dotnet");
            options.SupportedVersions = _environment.GetEnvironmentVariableAsList(
                EnvironmentSettingsKeys.DotnetCoreSupportedVersions);
            options.Project = _environment.GetEnvironmentVariable(EnvironmentSettingsKeys.Project);
        }
    }
}