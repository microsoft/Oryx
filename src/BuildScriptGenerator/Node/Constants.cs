// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    internal static class Constants
    {
        internal const string NodeJsName = "nodejs";
        internal const string PackageJsonFileName = "package.json";
        internal const string PackageLockJsonFileName = "package-lock.json";
        internal const string YarnLockFileName = "yarn.lock";
        internal const string NpmInstallCommand = "npm install";
        internal const string NpmRunBuildCommand = "npm run build";
        internal const string NpmRunBuildAzureCommand = "npm run build:azure";
        internal const string YarnInstallCommand = "yarn install";
    }
}