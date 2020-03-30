// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    public class PythonPlatformInstaller : PlatformInstallerBase
    {
        public PythonPlatformInstaller(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IEnvironment environment)
            : base(commonOptions, environment)
        {
        }

        public override string GetInstallerScriptSnippet(string version)
        {
            return GetInstallerScriptSnippet(PythonConstants.PlatformName, version);
        }

        public override bool IsVersionAlreadyInstalled(string version)
        {
            return IsVersionInstalled(
                version,
                builtInDir: PythonConstants.InstalledPythonVersionsDir,
                dynamicInstallDir: $"{Constants.TemporaryInstallationDirectoryRoot}/python");
        }
    }
}
