// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

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
            bool configureYarnCache = false,
            bool pruneDevDependencies = false,
            string appInsightsInjectCommand = null,
            string appInsightsPackageName = null,
            string appInsightsLoaderFileName = null,
            string packageInstallerVersionCommand = null)
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
            AppInsightsInjectCommand = appInsightsInjectCommand;
            AppInsightsPackageName = appInsightsPackageName;
            AppInsightsLoaderFileName = appInsightsLoaderFileName;
            PackageInstallerVersionCommand = packageInstallerVersionCommand;
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

        public string AppInsightsInjectCommand { get; set; }

        public string AppInsightsPackageName { get; set; }

        public string AppInsightsLoaderFileName { get; set; }

        public string PackageInstallerVersionCommand { get; set; }
    }
}