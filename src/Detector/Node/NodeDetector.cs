// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CSharp.RuntimeBinder;
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
        private readonly ILogger<NodeDetector> _logger;
        private readonly DetectorOptions _options;

        /// <summary>
        /// Creates an instance of <see cref="NodeDetector"/>.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger{NodeDetector}"/>.</param>
        /// <param name="options">The <see cref="DetectorOptions"/>.</param>
        public NodeDetector(ILogger<NodeDetector> logger, IOptions<DetectorOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        public PlatformDetectorResult Detect(DetectorContext context)
        {
            bool isNodeApp = false;
            bool hasLernaJsonFile = false;
            bool hasLageConfigJSFile = false;
            bool hasYarnrcYmlFile = false;
            bool IsYarnLockFileValidYamlFormat = false;
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
                _logger.LogDebug(
                    $"Could not find {NodeConstants.PackageJsonFileName}/{NodeConstants.PackageLockJsonFileName}" +
                    $"/{NodeConstants.YarnLockFileName} in repo");
            }
            if (sourceRepo.FileExists(NodeConstants.YarnrcYmlName))
            {
                hasYarnrcYmlFile = true;
            }
            if (sourceRepo.FileExists(NodeConstants.YarnLockFileName)
                && IsYarnLockFileYamlFile(sourceRepo, NodeConstants.YarnLockFileName)) {
                IsYarnLockFileValidYamlFormat = true;
            }
            if (sourceRepo.FileExists(NodeConstants.LernaJsonFileName))
            {
                hasLernaJsonFile = true;
                lernaNpmClient = GetLernaJsonNpmClient(context);
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
                            _logger.LogDebug(
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
                    _logger.LogDebug("Could not find typical Node.js files in repo");
                }
            }

            if (!isNodeApp)
            {
                _logger.LogDebug("App in repo is not a NodeJS app");
                return null;
            }

            var version = GetVersion(context);
            IEnumerable<FrameworkInfo> detectedFrameworkInfos = null;
            if (!_options.DisableFrameworkDetection)
            {
                detectedFrameworkInfos = DetectFrameworkInfos(context);
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
                IsYarnLockFileValidYamlFormat = IsYarnLockFileValidYamlFormat,
            };
        }

        private bool IsYarnLockFileYamlFile(ISourceRepo sourceRepo, string filePath)
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
            catch (Exception ex)
            {
                return false;
            }
        }

        private string GetVersion(DetectorContext context)
        {
            var version = GetVersionFromPackageJson(context);
            if (version != null)
            {
                return version;
            }
            _logger.LogDebug("Could not get version from package Json.");
            return null;
        }

        private string GetVersionFromPackageJson(DetectorContext context)
        {
            var packageJson = GetPackageJsonObject(context.SourceRepo, _logger);
            return packageJson?.engines?.node?.Value as string;
        }

        private dynamic GetPackageJsonObject(ISourceRepo sourceRepo, ILogger logger)
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

        private dynamic ReadJsonObjectFromFile(ISourceRepo sourceRepo, string fileName)
        {
            var jsonContent = sourceRepo.ReadFile(fileName);
            return JsonConvert.DeserializeObject(jsonContent);
        }

        private IEnumerable<FrameworkInfo> DetectFrameworkInfos(DetectorContext context)
        {
            var detectedFrameworkResult = new List<FrameworkInfo>();
            var packageJson = GetPackageJsonObject(context.SourceRepo, _logger);
            var monitoredDevDependencies = NodeConstants.DevDependencyFrameworkKeyWordToName;
            
            // dev-dependencies
            var devDependencies = packageJson?.devDependencies != null ? packageJson.devDependencies : new string[0];
            foreach (var dependency in devDependencies)
            {
                string dependencyName = dependency.Name;

                // wild-card dependency
                (bool isWildCardDependency, string wildCarddependencyName) = GetWildCardDependency(dependencyName);

                if (monitoredDevDependencies.ContainsKey(dependencyName) || isWildCardDependency) 
                {
                    var frameworkInfo = new FrameworkInfo
                    {
                        Framework = isWildCardDependency ? wildCarddependencyName : monitoredDevDependencies[dependencyName],
                        FrameworkVersion = dependency.Value.Value
                    };
                    detectedFrameworkResult.Add(frameworkInfo);
                }
            }

            var monitoredDependencies = NodeConstants.DependencyFrameworkKeyWordToName;
            // dependencies
            var dependencies = packageJson?.dependencies != null ? packageJson.dependencies : new string[0];
            foreach (var dependency in dependencies)
            {
                string dependencyName = dependency.Name;

                // wild-card dependency
                (bool isWildCardDependency, string wildCarddependencyName) = GetWildCardDependency(dependencyName);

                if (monitoredDependencies.ContainsKey(dependencyName) || isWildCardDependency)
                {
                    var frameworkInfo = new FrameworkInfo
                    {
                        Framework = isWildCardDependency ? wildCarddependencyName : monitoredDependencies[dependencyName],
                        FrameworkVersion = dependency.Value.Value
                    };
                    detectedFrameworkResult.Add(frameworkInfo);
                }
            }

            if (context.SourceRepo.FileExists(NodeConstants.FlutterYamlFileName)) {
                var frameworkInfo = new FrameworkInfo
                {
                    Framework = NodeConstants.FlutterFrameworkeName,
                    FrameworkVersion = string.Empty
                };
                detectedFrameworkResult.Add(frameworkInfo);
            }

            return detectedFrameworkResult;
        }

        private (bool, string) GetWildCardDependency(string dependencyName)
        {
            // wild-card dependenciy resolution examples:
            //      @angular/*  --> Angular
            //      @remix/*    --> Remix
            var wildCardDependencies = NodeConstants.WildCardDependencies;
            int forwardSlashIndex = dependencyName.IndexOf('/');
            bool isWildCardDependency = forwardSlashIndex > 0 &&
                wildCardDependencies.ContainsKey(dependencyName.Substring(0, forwardSlashIndex));
            string wildCardDepencyName = "";
            if (isWildCardDependency)
            {
                wildCardDepencyName = wildCardDependencies[dependencyName.Substring(0, forwardSlashIndex)];
            } 
            return (isWildCardDependency, wildCardDepencyName);
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
                    //Default Client for Lerna is npm.
                    npmClientName = NodeConstants.NpmToolName;
                }
            }
            catch (Exception exc)
            {
                // Leave malformed lerna.json files for Node.js to handle.
                // This prevents Oryx from erroring out when Node.js itself might be able to tolerate the file.
                _logger.LogWarning(
                    exc,
                    $"Exception caught while trying to deserialize {NodeConstants.LernaJsonFileName.Hash()}");
            }
            return npmClientName;
        }
    }
}