// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.SourceRepo;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    [BuildProperty(
        CompressNodeModulesPropertyKey,
        "Indicates how and if 'node_modules' folder should be compressed into a single file in the output folder. " +
        "Options are '" + ZipNodeModulesOption + "', and '" + TarGzNodeModulesOption + "'. Default is to not compress. " +
        "If this option is used, when running the app the node_modules folder must be extracted from this file.")]
    [BuildProperty(
        PruneDevDependenciesPropertyKey,
        "When using different intermediate and output folders, only the prod dependencies are copied to the output. " +
        "Options are 'true', blank (same meaning as 'true'), and 'false'. Default is false.")]
    internal class NodePlatform : IProgrammingPlatform
    {
        internal const string CompressNodeModulesPropertyKey = "compress_node_modules";
        internal const string PruneDevDependenciesPropertyKey = "prune_dev_dependencies";
        internal const string ZipNodeModulesOption = "zip";
        internal const string TarGzNodeModulesOption = "tar-gz";

        private readonly NodeScriptGeneratorOptions _nodeScriptGeneratorOptions;
        private readonly INodeVersionProvider _nodeVersionProvider;
        private readonly ILogger<NodePlatform> _logger;
        private readonly NodeLanguageDetector _detector;
        private readonly IEnvironment _environment;

        public NodePlatform(
            IOptions<NodeScriptGeneratorOptions> nodeScriptGeneratorOptions,
            INodeVersionProvider nodeVersionProvider,
            ILogger<NodePlatform> logger,
            NodeLanguageDetector detector,
            IEnvironment environment)
        {
            _nodeScriptGeneratorOptions = nodeScriptGeneratorOptions.Value;
            _nodeVersionProvider = nodeVersionProvider;
            _logger = logger;
            _detector = detector;
            _environment = environment;
        }

        public string Name => NodeConstants.NodeJsName;

        public IEnumerable<string> SupportedLanguageVersions => _nodeVersionProvider.SupportedNodeVersions;

        public LanguageDetectorResult Detect(ISourceRepo sourceRepo)
        {
            return _detector.Detect(sourceRepo);
        }

        public BuildScriptSnippet GenerateBashBuildScriptSnippet(BuildScriptGeneratorContext context)
        {
            var buildProperties = new Dictionary<string, string>();
            var packageJson = GetPackageJsonObject(context.SourceRepo, _logger);
            string runBuildCommand = null;
            string runBuildAzureCommand = null;
            bool configureYarnCache = false;
            string packageManagerCmd = null;
            string packageInstallCommand = null;

            if (context.SourceRepo.FileExists(NodeConstants.YarnLockFileName))
            {
                packageManagerCmd = NodeConstants.YarnCommand;
                packageInstallCommand = NodeConstants.YarnPackageInstallCommand;
                configureYarnCache = true;
            }
            else
            {
                packageManagerCmd = NodeConstants.NpmCommand;
                packageInstallCommand = NodeConstants.NpmPackageInstallCommand;
            }

            _logger.LogInformation("Using {packageManager}", packageManagerCmd);

            var hasProductionOnlyDependencies = false;
            if (packageJson?.devDependencies != null)
            {
                // If development time dependencies are present we want to avoid copying them to improve performance
                hasProductionOnlyDependencies = true;
            }

            var productionOnlyPackageInstallCommand = string.Format(
                NodeConstants.ProductionOnlyPackageInstallCommandTemplate, packageInstallCommand);

            var scriptsNode = packageJson?.scripts;
            if (scriptsNode != null)
            {
                if (scriptsNode.build != null)
                {
                    runBuildCommand = string.Format(NodeConstants.PkgMgrRunBuildCommandTemplate, packageManagerCmd);
                }

                if (scriptsNode["build:azure"] != null)
                {
                    runBuildAzureCommand = string.Format(
                        NodeConstants.PkgMgrRunBuildAzureCommandTemplate,
                        packageManagerCmd);
                }
            }

            if (packageJson?.dependencies != null)
            {
                Newtonsoft.Json.Linq.JObject deps = packageJson.dependencies;
                var depSpecs = deps.ToObject<IDictionary<string, string>>();
                _logger.LogDependencies(
                    context.Language,
                    context.NodeVersion,
                    depSpecs.Select(kv => kv.Key + kv.Value));
            }

            string compressNodeModulesCommand = null;
            string compressedNodeModulesFileName = null;
            GetNodeModulesPackOptions(context, out compressNodeModulesCommand, out compressedNodeModulesFileName);

            if (!string.IsNullOrWhiteSpace(compressedNodeModulesFileName))
            {
                buildProperties[NodeConstants.NodeModulesFileBuildProperty] = compressedNodeModulesFileName;
            }

            bool pruneDevDependencies = ShouldPruneDevDependencies(context);
            string appInsightsInjectCommand = string.Empty;
            var appInsightsKey = _environment.GetEnvironmentVariable(Constants.AppInsightsKey);
            var shouldInjectAppInsights = ShouldInjectAppInsights(packageJson, context, appInsightsKey);

            // node_options is only supported in version 8.0.0 or newer and in 6.12.0
            // so we will be able to set up app-insight only when node version is 6.12.0 or 8.0.0 or newer
            if (shouldInjectAppInsights)
            {
                appInsightsInjectCommand = string.Concat(
                    NodeConstants.NpmPackageInstallCommand,
                    " --save ",
                    NodeConstants.NodeAppInsightsPackageName);

                buildProperties[NodeConstants.InjectedAppInsights] = true.ToString();
                _logger.LogInformation("Oryx setting up Application Insights for auto-collection telemetry... ");
            }

            var scriptProps = new NodeBashBuildSnippetProperties(
                packageInstallCommand: packageInstallCommand,
                runBuildCommand: runBuildCommand,
                runBuildAzureCommand: runBuildAzureCommand,
                hasProductionOnlyDependencies: hasProductionOnlyDependencies,
                productionOnlyPackageInstallCommand: productionOnlyPackageInstallCommand,
                compressNodeModulesCommand: compressNodeModulesCommand,
                compressedNodeModulesFileName: compressedNodeModulesFileName,
                configureYarnCache: configureYarnCache,
                pruneDevDependencies: pruneDevDependencies,
                appInsightsInjectCommand: appInsightsInjectCommand,
                appInsightsPackageName: NodeConstants.NodeAppInsightsPackageName,
                appInsightsLoaderFileName: NodeAppInsightsLoader.NodeAppInsightsLoaderFileName);

            string script = TemplateHelpers.Render(
                TemplateHelpers.TemplateResource.NodeBuildSnippet,
                scriptProps,
                _logger);

            return new BuildScriptSnippet
            {
                BashBuildScriptSnippet = script,
                BuildProperties = buildProperties
            };
        }

        public bool IsCleanRepo(ISourceRepo repo)
        {
            return !repo.DirExists(NodeConstants.NodeModulesDirName);
        }

        public bool IsEnabled(BuildScriptGeneratorContext scriptGeneratorContext)
        {
            return scriptGeneratorContext.EnableNodeJs;
        }

        public bool IsEnabledForMultiPlatformBuild(BuildScriptGeneratorContext scriptGeneratorContext)
        {
            return true;
        }

        public void SetRequiredTools(
            ISourceRepo sourceRepo,
            string targetPlatformVersion,
            IDictionary<string, string> toolsToVersion)
        {
            Debug.Assert(toolsToVersion != null, $"{nameof(toolsToVersion)} must not be null");
            Debug.Assert(
                sourceRepo != null,
                $"{nameof(sourceRepo)} must not be null since Node needs access to the repository");
            if (!string.IsNullOrWhiteSpace(targetPlatformVersion))
            {
                toolsToVersion[NodeConstants.NodeToolName] = targetPlatformVersion;
            }

            var packageJson = GetPackageJsonObject(sourceRepo, _logger);
            if (packageJson != null)
            {
                string npmVersion = GetNpmVersion(packageJson);
                _logger.LogDebug("GetNpmVersion returned {npmVersion}", npmVersion);
                if (!string.IsNullOrEmpty(npmVersion))
                {
                    toolsToVersion[NodeConstants.NpmToolName] = npmVersion;
                }
            }
            else
            {
                _logger.LogDebug("packageJson is null");
            }
        }

        public void SetVersion(BuildScriptGeneratorContext context, string version)
        {
            context.NodeVersion = version;
        }

        public string GenerateBashRunScript(RunScriptGeneratorOptions options)
        {
            if (options.SourceRepo == null)
            {
                throw new ArgumentNullException(nameof(RunScriptGeneratorOptions.SourceRepo));
            }

            string startupCommand = null;

            // Log how we detected the entrypoint command
            var commandSource = string.Empty;
            if (!string.IsNullOrWhiteSpace(options.UserStartupCommand))
            {
                startupCommand = options.UserStartupCommand.Trim();
                _logger.LogInformation("Using user-provided startup command");
                commandSource = "User";
            }
            else
            {
                var packageJson = GetPackageJsonObject(options.SourceRepo, _logger);
                startupCommand = packageJson?.scripts?.start;
                if (string.IsNullOrWhiteSpace(startupCommand))
                {
                    string mainJsFile = packageJson?.main;
                    if (string.IsNullOrEmpty(mainJsFile))
                    {
                        var candidateFiles = new[]
                        {
                            "bin/www",
                            "server.js",
                            "app.js",
                            "index.js",
                            "hostingstart.js"
                        };
                        foreach (var file in candidateFiles)
                        {
                            if (options.SourceRepo.FileExists(file))
                            {
                                startupCommand = GetStartupCommandFromJsFile(options, file);
                                _logger.LogInformation("Found startup candidate {nodeStartupFile}", file);
                                commandSource = "CandidateFile";
                                break;
                            }
                        }
                    }
                    else
                    {
                        startupCommand = GetStartupCommandFromJsFile(options, mainJsFile);
                        commandSource = "PackageJsonMain";
                    }
                }
                else
                {
                    if (options.SourceRepo.FileExists(NodeConstants.YarnLockFileName))
                    {
                        commandSource = "PackageJsonStartYarn";
                        startupCommand = NodeConstants.YarnStartCommand;
                        _logger.LogInformation("Found startup command in package.json, and will use Yarn");
                    }
                    else
                    {
                        commandSource = "PackageJsonStartNpm";
                        startupCommand = NodeConstants.NpmStartCommand;
                        _logger.LogInformation("Found startup command in package.json, and will use npm");
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(startupCommand))
            {
                startupCommand = GetStartupCommandFromJsFile(options, options.DefaultAppPath);
                commandSource = "DefaultApp";
            }

            _logger.LogInformation("Finalizing entrypoint script using {commandSource}", commandSource);
            var templateValues = new NodeBashRunScriptProperties
            {
                AppDirectory = options.SourceRepo.RootPath,
                StartupCommand = startupCommand,
                ToolsVersions = string.IsNullOrWhiteSpace(options.PlatformVersion)
                ? null : $"node={options.PlatformVersion}",
                BindPort = options.BindPort
            };
            var script = TemplateHelpers.Render(TemplateHelpers.TemplateResource.NodeRunScript, templateValues);
            return script;
        }

        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToBuildOutputDir(
            BuildScriptGeneratorContext scriptGeneratorContext)
        {
            var dirs = new List<string>();
            dirs.Add(NodeConstants.AllNodeModulesDirName);
            dirs.Add(NodeConstants.ProdNodeModulesDirName);

            // If the node modules folder is being packaged in a file, we don't copy it to the output
            if (GetNodeModulesPackOptions(scriptGeneratorContext, out string compressCommand, out string compressedFileName))
            {
                dirs.Add(NodeConstants.NodeModulesDirName);
            }
            else if (!string.IsNullOrWhiteSpace(compressedFileName))
            {
                dirs.Add(compressedFileName);
            }

            return dirs;
        }

        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToIntermediateDir(
            BuildScriptGeneratorContext scriptGeneratorContext)
        {
            return new[]
            {
                NodeConstants.AllNodeModulesDirName,
                NodeConstants.ProdNodeModulesDirName,
                NodeConstants.NodeModulesDirName,
                NodeConstants.NodeModulesToBeDeletedName,
                NodeConstants.NodeModulesZippedFileName,
                NodeConstants.NodeModulesTarGzFileName
            };
        }

        internal static dynamic GetPackageJsonObject(ISourceRepo sourceRepo, ILogger logger)
        {
            dynamic packageJson = null;
            try
            {
                packageJson = sourceRepo.ReadJsonObjectFromFile(NodeConstants.PackageJsonFileName);
            }
            catch (Exception exc)
            {
                // Leave malformed package.json files for Node.js to handle.
                // This prevents Oryx from erroring out when Node.js itself might be able to tolerate the file.
                logger.LogWarning(
                    exc,
                    $"Exception caught while trying to deserialize {NodeConstants.PackageJsonFileName}");
            }

            return packageJson;
        }

        private static bool ShouldPruneDevDependencies(BuildScriptGeneratorContext context)
        {
            bool ret = false;
            if (context.Properties != null &&
                context.Properties.TryGetValue(PruneDevDependenciesPropertyKey, out string value))
            {
                if (string.IsNullOrWhiteSpace(value) || string.Equals("true", value, StringComparison.InvariantCultureIgnoreCase))
                {
                    ret = true;
                }
            }

            return ret;
        }

        private static bool DoesPackageDependencyExist(dynamic packageJson, string packageName)
        {
            if (packageJson?.dependencies != null)
            {
                Newtonsoft.Json.Linq.JObject deps = packageJson.dependencies;
                var pkgDependencies = deps.ToObject<IDictionary<string, string>>();
                if (pkgDependencies.ContainsKey(packageName))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ShouldInjectAppInsights(
            dynamic packageJson,
            BuildScriptGeneratorContext context,
            string appInsightsKey)
        {
            bool appInsightsDependency = DoesPackageDependencyExist(packageJson, NodeConstants.NodeAppInsightsPackageName);
            string appInsightsInjectCommand = string.Empty;

            // node_options is only supported in version 8.0 or newer and in 6.12
            // so we will be able to set up app-insight only when node version is 6.12 or 8.0 or newer
            if (!appInsightsDependency
                && !string.IsNullOrEmpty(appInsightsKey)
                && (SemanticVersionResolver.CompareVersions(context.NodeVersion, "8.0") >= 0
                || SemanticVersionResolver.CompareVersions(context.NodeVersion, "6.12") == 0
                || SemanticVersionResolver.CompareVersions(context.LanguageVersion, "8.0") >= 0
                || SemanticVersionResolver.CompareVersions(context.LanguageVersion, "6.12") == 0))
            {
                return true;
            }

                return false;
        }

        private static bool GetNodeModulesPackOptions(
            BuildScriptGeneratorContext context,
            out string compressNodeModulesCommand,
            out string compressedNodeModulesFileName)
        {
            var isNodeModulesPackaged = false;
            compressNodeModulesCommand = null;
            compressedNodeModulesFileName = null;
            if (context.Properties != null &&
                context.Properties.TryGetValue(CompressNodeModulesPropertyKey, out string compressNodeModulesOption))
            {
                // default to tar.gz if the property was provided with no value.
                if (string.IsNullOrEmpty(compressNodeModulesOption) ||
                    string.Equals(compressNodeModulesOption, TarGzNodeModulesOption, StringComparison.InvariantCultureIgnoreCase))
                {
                    compressedNodeModulesFileName = NodeConstants.NodeModulesTarGzFileName;
                    compressNodeModulesCommand = $"tar -zcf";
                    isNodeModulesPackaged = true;
                }
                else if (string.Equals(compressNodeModulesOption, ZipNodeModulesOption, StringComparison.InvariantCultureIgnoreCase))
                {
                    compressedNodeModulesFileName = NodeConstants.NodeModulesZippedFileName;
                    compressNodeModulesCommand = $"zip -y -q -r";
                    isNodeModulesPackaged = true;
                }
            }

            return isNodeModulesPackaged;
        }

        private string GetStartupCommandFromJsFile(RunScriptGeneratorOptions options, string file)
        {
            var command = string.Empty;
            if (!string.IsNullOrWhiteSpace(options.CustomServerCommand))
            {
                _logger.LogInformation("Using custom server command {nodeCommand}", options.CustomServerCommand);
                command = $"{options.CustomServerCommand.Trim()} {file}";
            }
            else
            {
                switch (options.DebuggingMode)
                {
                    case DebuggingMode.Standard:
                        _logger.LogInformation("Debugging in standard mode");
                        command = $"node --inspect {file}";
                        break;

                    case DebuggingMode.Break:
                        _logger.LogInformation("Debugging in break mode");
                        command = $"node --inspect-brk {file}";
                        break;

                    case DebuggingMode.None:
                        _logger.LogInformation("Running without debugging");
                        command = $"node {file}";
                        break;
                }
            }

            return command;
        }

        private string GetNpmVersion(dynamic packageJson)
        {
            string npmVersionRange = packageJson?.engines?.npm?.Value;
            if (npmVersionRange == null)
            {
                npmVersionRange = _nodeScriptGeneratorOptions.NpmDefaultVersion;
            }

            string npmVersion = null;
            if (!string.IsNullOrWhiteSpace(npmVersionRange))
            {
                var supportedNpmVersions = _nodeVersionProvider.SupportedNpmVersions;
                npmVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(npmVersionRange, supportedNpmVersions);
                if (string.IsNullOrEmpty(npmVersion))
                {
                    _logger.LogWarning(
                        "User requested npm version {npmVersion} but it wasn't resolved",
                        npmVersionRange);
                    return null;
                }
            }

            return npmVersion;
        }
    }
}