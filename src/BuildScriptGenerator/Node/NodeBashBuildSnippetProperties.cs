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
            bool zipNodeModulesDir,
            IEnumerable<string> directoriesToExcludeFromCopyToBuildOutputDir)
        {
            PackageInstallCommand = packageInstallCommand;
            NpmRunBuildCommand = runBuildCommand;
            NpmRunBuildAzureCommand = runBuildAzureCommand;
            HasProductionOnlyDependencies = hasProductionOnlyDependencies;
            ProductionOnlyPackageInstallCommand = productionOnlyPackageInstallCommand;
            ZipNodeModulesDir = zipNodeModulesDir;
            DirectoriesToExcludeFromCopyToBuildOutputDir = directoriesToExcludeFromCopyToBuildOutputDir;
        }

        public string PackageInstallCommand { get; set; }

        public string NpmRunBuildCommand { get; set; }

        public string NpmRunBuildAzureCommand { get; set; }

        public bool HasProductionOnlyDependencies { get; set; }

        public string ProductionOnlyPackageInstallCommand { get; set; }

        public bool ZipNodeModulesDir { get; set; }

        public IEnumerable<string> DirectoriesToExcludeFromCopyToBuildOutputDir { get; set; }
    }
}