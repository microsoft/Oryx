// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    internal static class NodeConstants
    {
        internal const string NodeJsName = "nodejs";
        internal const string PackageJsonFileName = "package.json";
        internal const string PackageLockJsonFileName = "package-lock.json";
        internal const string YarnLockFileName = "yarn.lock";
        internal const string NpmCommand = "npm";
        internal const string NpmStartCommand = "npm start";
        internal const string YarnStartCommand = "yarn run start";
        internal const string YarnCommand = "yarn";
        internal const string NpmPackageInstallCommand = "npm install";
        internal const string YarnPackageInstallCommand = "yarn install --prefer-offline";
        internal const string ProductionOnlyPackageInstallCommandTemplate = "{0} --production";
        internal const string PkgMgrRunBuildCommandTemplate = "{0} run build";
        internal const string PkgMgrRunBuildAzureCommandTemplate = "{0} run build:azure";
        internal const string AllNodeModulesDirName = "__oryx_all_node_modules";
        internal const string ProdNodeModulesDirName = "__oryx_prod_node_modules";
        internal const string NodeModulesDirName = "node_modules";
        internal const string NodeModulesZippedFileName = "node_modules.zip";
        internal const string NodeModulesTarGzFileName = "node_modules.tar.gz";
        internal const string NodeModulesFileBuildProperty = "compressedNodeModulesFile";
        internal const string CompressNodeModulesPropertyKey = "compress_node_modules";
        internal const string PruneDevDependenciesPropertyKey = "prune_dev_dependencies";
        internal const string ZipNodeModulesOption = "zip";
        internal const string TarGzNodeModulesOption = "tar-gz";
    }
}