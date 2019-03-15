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
using Newtonsoft.Json;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    internal class NodePlatform : IProgrammingPlatform
    {
        private readonly NodeScriptGeneratorOptions _nodeScriptGeneratorOptions;
        private readonly INodeVersionProvider _nodeVersionProvider;
        private readonly ILogger<NodePlatform> _logger;
        private readonly NodeLanguageDetector _detector;

        public NodePlatform(
            IOptions<NodeScriptGeneratorOptions> nodeScriptGeneratorOptions,
            INodeVersionProvider nodeVersionProvider,
            ILogger<NodePlatform> logger,
            NodeLanguageDetector detector)
        {
            _nodeScriptGeneratorOptions = nodeScriptGeneratorOptions.Value;
            _nodeVersionProvider = nodeVersionProvider;
            _logger = logger;
            _detector = detector;
        }

        public string Name => NodeConstants.NodeJsName;

        public IEnumerable<string> SupportedLanguageVersions => _nodeVersionProvider.SupportedNodeVersions;

        public LanguageDetectorResult Detect(ISourceRepo sourceRepo)
        {
            return _detector.Detect(sourceRepo);
        }

        public BuildScriptSnippet GenerateBashBuildScriptSnippet(BuildScriptGeneratorContext context)
        {
            var packageJson = GetPackageJsonObject(context.SourceRepo, _logger);
            string runBuildCommand = null;
            string runBuildAzureCommand = null;

            string packageManagerCmd = null;
            string packageInstallCommand = null;

            if (context.SourceRepo.FileExists(NodeConstants.YarnLockFileName))
            {
                packageManagerCmd = NodeConstants.YarnCommand;
                packageInstallCommand = NodeConstants.YarnPackageInstallCommand;
            }
            else
            {
                packageManagerCmd = NodeConstants.NpmCommand;
                packageInstallCommand = NodeConstants.NpmPackageInstallCommand;
            }

            _logger.LogInformation("Using {packageManager}", packageManagerCmd);

            var scriptsNode = packageJson?.scripts;
            if (scriptsNode != null)
            {
                if (scriptsNode.build != null)
                {
                    runBuildCommand = string.Format(NodeConstants.PkgMgrRunBuildCommandTemplate, packageManagerCmd);
                }

                if (scriptsNode["build:azure"] != null)
                {
                    runBuildAzureCommand = string.Format(NodeConstants.PkgMgrRunBuildAzureCommandTemplate, packageManagerCmd);
                }
            }

            if (packageJson?.dependencies != null)
            {
                Newtonsoft.Json.Linq.JObject deps = packageJson.dependencies;
                var depSpecs = deps.ToObject<IDictionary<string, string>>();
                _logger.LogDependencies(context.Language, context.NodeVersion, depSpecs.Select(kv => kv.Key + kv.Value));
            }

            var scriptProps = new NodeBashBuildSnippetProperties(
                packageInstallCommand: packageInstallCommand,
                runBuildCommand: runBuildCommand,
                runBuildAzureCommand: runBuildAzureCommand,
                zipNodeModulesDir: _nodeScriptGeneratorOptions.ZipNodeModules);
            string script = TemplateHelpers.Render(TemplateHelpers.TemplateResource.NodeBuildSnippet, scriptProps, _logger);

            return new BuildScriptSnippet { BashBuildScriptSnippet = script };
        }

        public bool IsCleanRepo(ISourceRepo repo)
        {
            return !repo.DirExists(NodeConstants.NodeModulesDirName);
        }

        public bool IsEnabled(BuildScriptGeneratorContext scriptGeneratorContext)
        {
            return scriptGeneratorContext.EnableNodeJs;
        }

        public void SetRequiredTools(ISourceRepo sourceRepo, string targetPlatformVersion, IDictionary<string, string> toolsToVersion)
        {
            Debug.Assert(toolsToVersion != null, $"{nameof(toolsToVersion)} must not be null");
            Debug.Assert(sourceRepo != null, $"{nameof(sourceRepo)} must not be null since Node needs access to the repository");
            if (!string.IsNullOrWhiteSpace(targetPlatformVersion))
            {
                toolsToVersion["node"] = targetPlatformVersion;
            }

            var packageJson = GetPackageJsonObject(sourceRepo, _logger);
            if (packageJson != null)
            {
                var npmVersion = GetNpmVersion(packageJson);
                if (!string.IsNullOrEmpty(npmVersion))
                {
                    toolsToVersion["npm"] = npmVersion;
                }
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
                        var candidateFiles = new[] { "bin/www", "server.js", "app.js", "index.js", "hostingstart.js" };
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
                ToolsVersions = string.IsNullOrWhiteSpace(options.PlatformVersion) ? null : $"node={options.PlatformVersion}",
                BindPort = options.BindPort
            };
            var script = TemplateHelpers.Render(TemplateHelpers.TemplateResource.NodeRunScript, templateValues);
            return script;
        }

        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToBuildOutputDir()
        {
            var dirs = new List<string>();
            if (_nodeScriptGeneratorOptions.ZipNodeModules)
            {
                dirs.Add(NodeConstants.NodeModulesDirName);
            }
            else
            {
                dirs.Add(NodeConstants.NodeModulesZippedFileName);
            }

            return dirs;
        }

        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToIntermediateDir()
        {
            return new[] { NodeConstants.NodeModulesDirName, NodeConstants.NodeModulesZippedFileName };
        }

        internal static dynamic GetPackageJsonObject(ISourceRepo sourceRepo, ILogger logger)
        {
            dynamic packageJson = null;
            try
            {
                var jsonContent = sourceRepo.ReadFile(NodeConstants.PackageJsonFileName);
                packageJson = JsonConvert.DeserializeObject(jsonContent);
            }
            catch (Exception exc)
            {
                // Leave malformed package.json files for Node.js to handle.
                // This prevents Oryx from erroring out when Node.js itself might be able to tolerate the file.
                logger.LogWarning(exc, $"Exception caught while trying to deserialize {NodeConstants.PackageJsonFileName}");
            }

            return packageJson;
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
                npmVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(
                    npmVersionRange,
                    supportedNpmVersions);
                if (string.IsNullOrWhiteSpace(npmVersion))
                {
                    _logger.LogWarning("User requested npm version {npmVersion} but it wasn't resolved", npmVersionRange);
                    return null;
                }
            }

            return npmVersion;
        }
    }
}