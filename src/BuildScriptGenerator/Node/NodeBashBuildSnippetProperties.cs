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
        public string PackageRegistryUrl { get; set; }

        public string PackageInstallCommand { get; set; }

        public string NpmRunBuildCommand { get; set; }

        public string CustomRunBuildCommand { get; set; }

        public string NpmRunBuildAzureCommand { get; set; }

        public bool HasProdDependencies { get; set; }

        public bool HasDevDependencies { get; set; }

        public string ProductionOnlyPackageInstallCommand { get; set; }

        public string CompressNodeModulesCommand { get; set; }

        public string CompressedNodeModulesFileName { get; set; }

        public bool ConfigureYarnCache { get; set; }

        public bool PruneDevDependencies { get; set; }

        public string AppInsightsInjectCommand { get; set; }

        public string AppInsightsPackageName { get; set; }

        public string AppInsightsLoaderFileName { get; set; }

        public string PackageInstallerVersionCommand { get; set; }

        public bool RunNpmPack { get; set; }
    }
}