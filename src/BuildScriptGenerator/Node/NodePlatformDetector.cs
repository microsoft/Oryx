// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    internal class NodePlatformDetector : IPlatformDetector
    {
        private static readonly string[] IisStartupFiles = new[]
        {
            "default.htm",
            "default.html",
            "default.asp",
            "index.htm",
            "index.html",
            "iisstart.htm",
            "default.aspx",
            "index.php",
        };

        private static readonly string[] TypicalNodeDetectionFiles = new[]
        {
            "server.js",
            "app.js",
        };

        private readonly INodeVersionProvider _versionProvider;
        private readonly NodeScriptGeneratorOptions _options;
        private readonly ILogger<NodePlatformDetector> _logger;
        private readonly IEnvironment _environment;
        private readonly IStandardOutputWriter _writer;

        public NodePlatformDetector(
            INodeVersionProvider nodeVersionProvider,
            IOptions<NodeScriptGeneratorOptions> options,
            ILogger<NodePlatformDetector> logger,
            IEnvironment environment,
            IStandardOutputWriter writer)
        {
            _versionProvider = nodeVersionProvider;
            _options = options.Value;
            _logger = logger;
            _environment = environment;
            _writer = writer;
        }

        public PlatformDetectorResult Detect(RepositoryContext context)
        {
            bool isNodeApp = false;

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

            if (!isNodeApp)
            {
                // Copying the logic currently running in Kudu:
                var mightBeNode = false;
                foreach (var typicalNodeFile in TypicalNodeDetectionFiles)
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
                    foreach (var iisStartupFile in IisStartupFiles)
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

            var version = GetVersionFromPackageJson(context);

            return new PlatformDetectorResult
            {
                Platform = NodeConstants.PlatformName,
                PlatformVersion = version,
            };
        }

        private string GetVersionFromPackageJson(RepositoryContext context)
        {
            var packageJson = NodePlatform.GetPackageJsonObject(context.SourceRepo, _logger);
            return packageJson?.engines?.node?.Value as string;
        }
    }
}