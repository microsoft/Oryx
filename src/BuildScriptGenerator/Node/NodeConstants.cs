// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    public static class NodeConstants
    {
        public const string PlatformName = "nodejs";
        public const string PackageJsonFileName = "package.json";
        public const string PackageLockJsonFileName = "package-lock.json";
        public const string YarnLockFileName = "yarn.lock";
        public const string NodeToolName = "node";
        public const string NpmToolName = "npm";
        public const string NpmCommand = NpmToolName;
        public const string NpmStartCommand = "npm start";
        public const string YarnStartCommand = "yarn run start";
        public const string YarnCommand = "yarn";
        public const string HugoCommand = "hugo";
        public const string NpmPackageInstallCommand = "npm install --unsafe-perm";
        public const string NpmVersionCommand = "echo Using Npm version: && npm --version";
        public const string YarnVersionCommand = "echo Using Yarn version: && yarn --version";
        public const string HugoVersionCommand = "echo Using Hugo version: && hugo version";
        public const string YarnPackageInstallCommand = "yarn install --prefer-offline";
        public const string ProductionOnlyPackageInstallCommandTemplate = "{0} --production";
        public const string PkgMgrRunBuildCommandTemplate = "{0} run build";
        public const string PkgMgrRunBuildAzureCommandTemplate = "{0} run build:azure";
        public const string AllNodeModulesDirName = ".oryx_all_node_modules";
        public const string ProdNodeModulesDirName = ".oryx_prod_node_modules";
        public const string NodeModulesDirName = "node_modules";
        public const string NodeModulesToBeDeletedName = "_del_node_modules";
        public const string NodeModulesZippedFileName = "node_modules.zip";
        public const string NodeModulesTarGzFileName = "node_modules.tar.gz";
        public const string NodeModulesFileBuildProperty = "compressedNodeModulesFile";
        public const string NodeAppInsightsPackageName = "applicationinsights";
        public const string InjectedAppInsights = "injectedAppInsights";
        public const string NodeLtsVersion = NodeVersions.Node12Version;
        public const string InstalledNodeVersionsDir = "/opt/nodejs/";
        public const string NodeVersion = "NODE_VERSION";
        public const string LegacyZipNodeModules = "ENABLE_NODE_MODULES_ZIP";
    }
}