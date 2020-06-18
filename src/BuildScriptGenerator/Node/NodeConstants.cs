// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    internal static class NodeConstants
    {
        internal const string PlatformName = "nodejs";
        internal const string PackageJsonFileName = "package.json";
        internal const string PackageLockJsonFileName = "package-lock.json";
        internal const string YarnLockFileName = "yarn.lock";
        internal const string NodeToolName = "node";
        internal const string NpmToolName = "npm";
        internal const string NpmCommand = NpmToolName;
        internal const string NpmStartCommand = "npm start";
        internal const string YarnStartCommand = "yarn run start";
        internal const string YarnCommand = "yarn";
        internal const string HugoCommand = "hugo";
        internal const string NpmPackageInstallCommand = "npm install --unsafe-perm";
        internal const string NpmVersionCommand = "echo Using Npm version: && npm --version";
        internal const string YarnVersionCommand = "echo Using Yarn version: && yarn --version";
        internal const string HugoVersionCommand = "echo Using Hugo version: && hugo version";
        internal const string YarnPackageInstallCommand = "yarn install --prefer-offline";
        internal const string ProductionOnlyPackageInstallCommandTemplate = "{0} --production";
        internal const string PkgMgrRunBuildCommandTemplate = "{0} run build";
        internal const string PkgMgrRunBuildAzureCommandTemplate = "{0} run build:azure";
        internal const string AllNodeModulesDirName = "__oryx_all_node_modules";
        internal const string ProdNodeModulesDirName = "__oryx_prod_node_modules";
        internal const string NodeModulesDirName = "node_modules";
        internal const string NodeModulesToBeDeletedName = "_del_node_modules";
        internal const string NodeModulesZippedFileName = "node_modules.zip";
        internal const string NodeModulesTarGzFileName = "node_modules.tar.gz";
        internal const string NodeModulesFileBuildProperty = "compressedNodeModulesFile";
        internal const string NodeAppInsightsPackageName = "applicationinsights";
        internal const string InjectedAppInsights = "injectedAppInsights";
        internal const string NodeLtsVersion = NodeVersions.Node12Version;
        internal const string InstalledNodeVersionsDir = "/opt/nodejs/";
        internal const string NodeVersion = "NODE_VERSION";
        internal const string LegacyZipNodeModules = "ENABLE_NODE_MODULES_ZIP";
    }
}