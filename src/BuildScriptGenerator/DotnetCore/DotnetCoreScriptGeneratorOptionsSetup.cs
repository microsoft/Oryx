// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.DotnetCore
{
    internal class DotnetCoreScriptGeneratorOptionsSetup : IConfigureOptions<DotnetCoreScriptGeneratorOptions>
    {
        internal const string DefaultVersion = DotnetCoreConstants.DotnetCoreSdkVersion21;
        internal const string InstalledVersionsDir = "/opt/dotnet/";

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
        }
    }
}