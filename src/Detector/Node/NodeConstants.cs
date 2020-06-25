// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.Detector.Node
{
    internal static class NodeConstants
    {
        public const string PlatformName = "nodejs";
        public const string PackageJsonFileName = "package.json";
        public const string PackageLockJsonFileName = "package-lock.json";
        public const string YarnLockFileName = "yarn.lock";
        public const string HugoTomlFileName = "config.toml";
        public const string HugoYamlFileName = "config.yaml";
        public const string HugoJsonFileName = "config.json";
        public const string HugoConfigFolderName = "config";
        public const string NodeModulesDirName = "node_modules";
        public const string NodeModulesToBeDeletedName = "_del_node_modules";
        public const string NodeModulesZippedFileName = "node_modules.zip";
        public const string NodeModulesTarGzFileName = "node_modules.tar.gz";
        public const string NodeModulesFileBuildProperty = "compressedNodeModulesFile";
    }
}