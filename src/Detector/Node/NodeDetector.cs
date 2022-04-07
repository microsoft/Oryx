// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Common.Extensions;
using Newtonsoft.Json;
using YamlDotNet.RepresentationModel;

namespace Microsoft.Oryx.Detector.Node
{
    /// <summary>
    /// An implementation of <see cref="IPlatformDetector"/> which detects NodeJS applications.
    /// </summary>
    public class NodeDetector : INodePlatformDetector
    {
        private readonly ILogger<NodeDetector> logger;
        private readonly DetectorOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeDetector"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger{NodeDetector}"/>.</param>
        /// <param name="options">The <see cref="DetectorOptions"/>.</param>
        public NodeDetector(ILogger<NodeDetector> logger, IOptions<DetectorOptions> options)
        {
            this.logger = logger;
            this.options = options.Value;
        }

        public PlatformDetectorResult Detect(DetectorContext context)
        {
            bool isNodeApp = false;
            bool hasLernaJsonFile = false;
            bool hasLageConfigJSFile = false;
            bool hasYarnrcYmlFile = false;
            bool isYarnLockFileValidYamlFormat = false;
            string appDirectory = string.Empty;
            string lernaNpmClient = string.Empty;
            var sourceRepo = context.SourceRepo;
            if (sourceRepo.FileExists(NodeConstants.PackageJsonFileName) ||
                sourceRepo.FileExists(NodeConstants.PackageLockJsonFileName) ||
                sourceRepo.FileExists(NodeConstants.YarnLockFileName))
            {
                isNodeApp = true;
            }
            else
            {
                this.logger.LogDebug(
                    $"Could not find {NodeConstants.PackageJsonFileName}/{NodeConstants.PackageLockJsonFileName}" +
                    $"/{NodeConstants.YarnLockFileName} in repo");
            }

            if (sourceRepo.FileExists(NodeConstants.YarnrcYmlName))
            {
                hasYarnrcYmlFile = true;
            }

            if (sourceRepo.FileExists(NodeConstants.YarnLockFileName)
                && IsYarnLockFileYamlFile(sourceRepo, NodeConstants.YarnLockFileName))
            {
                isYarnLockFileValidYamlFormat = true;
            }

            if (sourceRepo.FileExists(NodeConstants.LernaJsonFileName))
            {
                hasLernaJsonFile = true;
                lernaNpmClient = this.GetLernaJsonNpmClient(context);
            }

            if (sourceRepo.FileExists(NodeConstants.LageConfigJSFileName))
            {
                hasLageConfigJSFile = true;
            }

            if (!isNodeApp)
            {
                // Copying the logic currently running in Kudu:
                var mightBeNode = false;
                foreach (var typicalNodeFile in NodeConstants.TypicalNodeDetectionFiles)
                {
                    if (sourceRepo.FileExists(typicalNodeFile))
                    {
                        mightBeNode = true;
                        break;
                    }
                }

                if (mightBeNode)
                {
                    // Check if any of the known iis start pages exist
                    // If so, then it is not a node.js web site otherwise it is
                    foreach (var iisStartupFile in NodeConstants.IisStartupFiles)
                    {
                        if (sourceRepo.FileExists(iisStartupFile))
                        {
                            this.logger.LogDebug(
                                "App in repo is not a Node.js app as it has the file {iisStartupFile}",
                                iisStartupFile.Hash());
                            return null;
                        }
                    }

                    isNodeApp = true;
                }
                else
                {
                    // No point in logging the actual file list, as it's constant
                    this.logger.LogDebug("Could not find typical Node.js files in repo");
                }
            }

            if (!isNodeApp)
            {
                this.logger.LogDebug("App in repo is not a NodeJS app");
                return null;
            }

            var version = this.GetVersion(context);
            IEnumerable<FrameworkInfo> detectedFrameworkInfos = null;
            if (!this.options.DisableFrameworkDetection)
            {
                detectedFrameworkInfos = this.DetectFrameworkInfos(context);
            }

            return new NodePlatformDetectorResult
            {
                Platform = NodeConstants.PlatformName,
                PlatformVersion = version,
                AppDirectory = appDirectory,
                Frameworks = detectedFrameworkInfos,
                HasLernaJsonFile = hasLernaJsonFile,
                HasLageConfigJSFile = hasLageConfigJSFile,
                LernaNpmClient = lernaNpmClient,
                HasYarnrcYmlFile = hasYarnrcYmlFile,
                IsYarnLockFileValidYamlFormat = isYarnLockFileValidYamlFormat,
            };
        }

        private static bool IsYarnLockFileYamlFile(ISourceRepo sourceRepo, string filePath)
        {
            try
            {
                using (var reader = new StringReader(sourceRepo.ReadFile(filePath)))
                {
                    var yamlStream = new YamlStream();
                    yamlStream.Load(reader);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static dynamic GetPackageJsonObject(ISourceRepo sourceRepo, ILogger logger)
        {
            dynamic packageJson = null;
            try
            {
                packageJson = ReadJsonObjectFromFile(sourceRepo, NodeConstants.PackageJsonFileName);
            }
            catch (Exception exc)
            {
                // Leave malformed package.json files for Node.js to handle.
                // This prevents Oryx from erroring out when Node.js itself might be able to tolerate the file.
                logger.LogWarning(
                    exc,
                    $"Exception caught while trying to deserialize {NodeConstants.PackageJsonFileName.Hash()}");
            }

            return packageJson;
        }

        private static dynamic ReadJsonObjectFromFile(ISourceRepo sourceRepo, string fileName)
        {
            var jsonContent = sourceRepo.ReadFile(fileName);
            return JsonConvert.DeserializeObject(jsonContent);
        }

        private static (bool IsWildCardDependency, string WildCardDependencyName) GetWildCardDependency(string dependencyName)
        {
            // wild-card dependenciy resolution examples:
            //      @angular/*  --> Angular
            //      @remix/*    --> Remix
            var wildCardDependencies = NodeConstants.WildCardDependencies;
            int forwardSlashIndex = dependencyName.IndexOf('/');
            bool isWildCardDependency = forwardSlashIndex > 0 &&
                wildCardDependencies.ContainsKey(dependencyName.Substring(0, forwardSlashIndex));
            string wildCardDepencyName = string.Empty;
            if (isWildCardDependency)
            {
                wildCardDepencyName = wildCardDependencies[dependencyName.Substring(0, forwardSlashIndex)];
            }

            return (isWildCardDependency, wildCardDepencyName);
        }

        private string GetVersion(DetectorContext context)
        {
            var version = this.GetVersionFromPackageJson(context);
            if (version != null)
            {
                return version;
            }

            this.logger.LogDebug("Could not get version from package Json.");
            return null;
        }

        private string GetVersionFromPackageJson(DetectorContext context)
        {
            var packageJson = GetPackageJsonObject(context.SourceRepo, this.logger);
            return packageJson?.engines?.node?.Value as string;
        }

        private IEnumerable<FrameworkInfo> DetectFrameworkInfos(DetectorContext context)
        {
            // TODO: consolidate dependency & dev-dependency logic
            //       work-item 1493329
            var detectedFrameworkResult = new List<FrameworkInfo>();
            var packageJson = GetPackageJsonObject(context.SourceRepo, this.logger);
            var monitoredDevDependencies = NodeConstants.DevDependencyFrameworkKeyWordToName;

            // frameworksSet is for preventing duplicates
            var frameworksSet = new HashSet<string>();

            // dev-dependencies
            var devDependencies = packageJson?.devDependencies != null ? packageJson.devDependencies : Array.Empty<string>();
            foreach (var dependency in devDependencies)
            {
                string dependencyName = dependency.Name;

                // wild-card dependency
                (bool isWildCardDependency, string wildCardDependencyName) = GetWildCardDependency(dependencyName);

                if (!monitoredDevDependencies.ContainsKey(dependencyName) && !isWildCardDependency)
                {
                    continue;
                }

                string frameworkName = isWildCardDependency ? wildCardDependencyName : monitoredDevDependencies[dependencyName];
                if (!frameworksSet.Contains(frameworkName))
                {
                    var frameworkInfo = new FrameworkInfo
                    {
                        Framework = frameworkName,
                        FrameworkVersion = dependency.Value.Value,
                    };
                    detectedFrameworkResult.Add(frameworkInfo);
                    frameworksSet.Add(frameworkName);
                }
            }

            var monitoredDependencies = NodeConstants.DependencyFrameworkKeyWordToName;

            // dependencies
            var dependencies = packageJson?.dependencies != null ? packageJson.dependencies : Array.Empty<string>();
            foreach (var dependency in dependencies)
            {
                string dependencyName = dependency.Name;

                // wild-card dependency
                (bool isWildCardDependency, string wildCardDependencyName) = GetWildCardDependency(dependencyName);

                if (!monitoredDependencies.ContainsKey(dependencyName) && !isWildCardDependency)
                {
                    continue;
                }

                string frameworkName = isWildCardDependency ? wildCardDependencyName : monitoredDependencies[dependencyName];
                if (!frameworksSet.Contains(frameworkName))
                {
                    var frameworkInfo = new FrameworkInfo
                    {
                        Framework = frameworkName,
                        FrameworkVersion = dependency.Value.Value,
                    };
                    detectedFrameworkResult.Add(frameworkInfo);
                    frameworksSet.Add(frameworkName);
                }
            }

            if (context.SourceRepo.FileExists(NodeConstants.FlutterYamlFileName))
            {
                var frameworkInfo = new FrameworkInfo
                {
                    Framework = NodeConstants.FlutterFrameworkeName,
                    FrameworkVersion = string.Empty,
                };
                detectedFrameworkResult.Add(frameworkInfo);
            }

            // remove base frameworks if derived framework exists
            if (frameworksSet.Contains("Gatsby") || frameworksSet.Contains("Next.js"))
            {
                detectedFrameworkResult.RemoveAll(x => x.Framework == "Angular");
                detectedFrameworkResult.RemoveAll(x => x.Framework == "React");
            }

            if (frameworksSet.Contains("VuePress") || frameworksSet.Contains("Nuxt.js"))
            {
                detectedFrameworkResult.RemoveAll(x => x.Framework == "Vue.js");
            }

            return detectedFrameworkResult;
        }

        private string GetLernaJsonNpmClient(DetectorContext context)
        {
            var npmClientName = string.Empty;
            if (!context.SourceRepo.FileExists(NodeConstants.LernaJsonFileName))
            {
                return npmClientName;
            }

            try
            {
                dynamic lernaJson = ReadJsonObjectFromFile(context.SourceRepo, NodeConstants.LernaJsonFileName);
                if (lernaJson?.npmClient != null)
                {
                    npmClientName = lernaJson["npmClient"].Value as string;
                }
                else
                {
                    // Default Client for Lerna is npm.
                    npmClientName = NodeConstants.NpmToolName;
                }
            }
            catch (Exception exc)
            {
                // Leave malformed lerna.json files for Node.js to handle.
                // This prevents Oryx from erroring out when Node.js itself might be able to tolerate the file.
                this.logger.LogWarning(
                    exc,
                    $"Exception caught while trying to deserialize {NodeConstants.LernaJsonFileName.Hash()}");
            }

            return npmClientName;
        }
    }
}
