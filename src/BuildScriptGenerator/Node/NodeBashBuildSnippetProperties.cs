// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    /// <summary>
    /// Build script template for NodeJs in Bash.
    /// </summary>
    public class NodeBashBuildSnippetProperties
    {
        public NodeBashBuildSnippetProperties(
            string packageInstallCommand,
            string runBuildCommand,
            string runBuildAzureCommand,
            bool hasProductionOnlyDependencies,
            string productionOnlyPackageInstallCommand,
            string compressNodeModulesCommand,
            string compressedNodeModulesFileName,
            IEnumerable<string> directoriesToExcludeFromCopyToBuildOutputDir,
            bool configureYarnCache = false,
            bool pruneDevDependencies = false,
            bool zipAllOutput = false)
        {
            PackageInstallCommand = packageInstallCommand;
            NpmRunBuildCommand = runBuildCommand;
            NpmRunBuildAzureCommand = runBuildAzureCommand;
            HasProductionOnlyDependencies = hasProductionOnlyDependencies;
            ProductionOnlyPackageInstallCommand = productionOnlyPackageInstallCommand;
            CompressNodeModulesCommand = compressNodeModulesCommand;
            CompressedNodeModulesFileName = compressedNodeModulesFileName;
            ConfigureYarnCache = configureYarnCache;
            PruneDevDependencies = pruneDevDependencies;
            DirectoriesToExcludeFromCopyToBuildOutputDir = directoriesToExcludeFromCopyToBuildOutputDir;
            ZipAllOutput = zipAllOutput;
        }

        public string PackageInstallCommand { get; set; }

        public string NpmRunBuildCommand { get; set; }

        public string NpmRunBuildAzureCommand { get; set; }

        public bool HasProductionOnlyDependencies { get; set; }

        public string ProductionOnlyPackageInstallCommand { get; set; }

        public string CompressNodeModulesCommand { get; set; }

        public string CompressedNodeModulesFileName { get; set; }

        public bool ConfigureYarnCache { get; set; }

        public bool PruneDevDependencies { get; set; }

        public IEnumerable<string> DirectoriesToExcludeFromCopyToBuildOutputDir { get; set; }

        public bool ZipAllOutput { get; set; }
    }
}