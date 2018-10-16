// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Newtonsoft.Json;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    internal class NodeScriptGenerator : IScriptGenerator
    {
        private const string NodeJsName = "nodejs";
        private const string PackageJsonFileName = "package.json";

        // NOTE: C# multiline strings are handled verbatim, so if you place a tab to the text here,
        // a tab would be present in the generated output too.
        private const string ScriptTemplate =
            @"#!/bin/bash
#set -ex

SOURCE_DIR=$1
DESTINATION_DIR=$2

if [ ! -d ""$SOURCE_DIR"" ]; then
    echo ""Source directory '$SOURCE_DIR' does not exist."" 1>&2
    exit 1
fi

if [ -z ""$DESTINATION_DIR"" ]
then
    DESTINATION_DIR=""$SOURCE_DIR""
fi

# Get full file paths to source and destination directories
cd $SOURCE_DIR
SOURCE_DIR=$(pwd -P)

if [ -d ""$DESTINATION_DIR"" ]
then
    cd $DESTINATION_DIR
    DESTINATION_DIR=$(pwd -P)
fi

echo
echo ""Source directory     : $SOURCE_DIR""
echo ""Destination directory: $DESTINATION_DIR""
echo

source /usr/local/bin/benv {0}

echo Installing npm packages ...
cd ""$SOURCE_DIR""
echo
echo ""Running '{1}' ...""
echo
{1}

if [ ""$SOURCE_DIR"" == ""$DESTINATION_DIR"" ]
then
    echo Done.
    exit 0
fi

if [ -d ""$DESTINATION_DIR"" ]
then
    echo
    echo Destination directory already exists. Deleting it ...
    rm -rf ""$DESTINATION_DIR""
fi

appTempDir=`mktemp -d`
cp -rf ""$SOURCE_DIR""/* ""$appTempDir""
mkdir -p ""$DESTINATION_DIR""
cp -rf ""$appTempDir""/* ""$DESTINATION_DIR""
rm -rf ""$appTempDir""

echo
echo Done.
";

        private static readonly string[] IisStartupFiles = new[]
        {
            "default.htm",
            "default.html",
            "default.asp",
            "index.htm",
            "index.html",
            "iisstart.htm",
            "default.aspx",
            "index.php"
        };

        private static readonly string[] TypicalNodeDetectionFiles = new[] { "server.js", "app.js" };
        private readonly NodeScriptGeneratorOptions _nodeScriptGeneratorOptions;
        private readonly INodeVersionProvider _nodeVersionProvider;
        private readonly ILogger<NodeScriptGenerator> _logger;

        public NodeScriptGenerator(
            IOptions<NodeScriptGeneratorOptions> nodeScriptGeneratorOptions,
            INodeVersionProvider nodeVersionProvider,
            ILogger<NodeScriptGenerator> logger)
        {
            _nodeScriptGeneratorOptions = nodeScriptGeneratorOptions.Value;
            _nodeVersionProvider = nodeVersionProvider;
            _logger = logger;
        }

        public string SupportedLanguageName => NodeJsName;

        public IEnumerable<string> SupportedLanguageVersions => _nodeVersionProvider.SupportedNodeVersions;

        public bool CanGenerateScript(ScriptGeneratorContext context)
        {
            if (context.SourceRepo.FileExists(PackageJsonFileName))
            {
                return true;
            }
            else
            {
                _logger.LogDebug($"Could not find file '{PackageJsonFileName}' in the source directory.");
            }

            // Copying the logic currently running in Kudu:
            var mightBeNode = false;
            foreach (var typicalNodeFile in TypicalNodeDetectionFiles)
            {
                if (context.SourceRepo.FileExists(typicalNodeFile))
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
                    if (context.SourceRepo.FileExists(iisStartupFile))
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                _logger.LogDebug(
                    $"Could not find following typical node files in the source directory: " +
                    string.Join(", ", TypicalNodeDetectionFiles));
            }

            return false;
        }

        public string GenerateBashScript(ScriptGeneratorContext context)
        {
            (var nodeVersion, var npmVersion) = DetectVersionInformation(context);

            string benvArgs = string.Empty;
            if (!string.IsNullOrEmpty(nodeVersion))
            {
                benvArgs += $"node={nodeVersion} ";
            }

            if (!string.IsNullOrEmpty(npmVersion))
            {
                benvArgs += $"npm={npmVersion} ";
            }

            var installCommand = "eval npm install --production";

            var script = string.Format(ScriptTemplate, benvArgs, installCommand);
            return script;
        }

        private (string nodeVersion, string npmVersion) DetectVersionInformation(ScriptGeneratorContext context)
        {
            string nodeVersion;
            string npmVersion;
            var packageJson = GetPackageJsonObject(context.SourceRepo);
            if (string.IsNullOrEmpty(context.LanguageVersion))
            {
                nodeVersion = DetectNodeVersion(packageJson);
            }
            else
            {
                nodeVersion = context.LanguageVersion;
            }

            npmVersion = DetectNpmVersion(packageJson);

            return (nodeVersion, npmVersion);
        }

        private string DetectNodeVersion(dynamic packageJson)
        {
            var nodeVersionRange = packageJson?.engines?.node?.Value as string;
            if (nodeVersionRange == null)
            {
                nodeVersionRange = _nodeScriptGeneratorOptions.NodeJsDefaultVersion;
            }
            string nodeVersion = null;
            if (!string.IsNullOrWhiteSpace(nodeVersionRange))
            {
                nodeVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(
                    nodeVersionRange,
                    _nodeVersionProvider.SupportedNodeVersions);
                if (string.IsNullOrWhiteSpace(nodeVersion))
                {
                    var message = $"The target Node.js version '{nodeVersionRange}' is not supported. " +
                        $"Supported versions are: {string.Join(", ", SupportedLanguageVersions)}";

                    _logger.LogError(message);
                    throw new UnsupportedVersionException(message);
                }
            }
            return nodeVersion;
        }

        private string DetectNpmVersion(dynamic packageJson)
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
                    var message = $"The target npm version '{npmVersionRange}' is not supported. " +
                        $"Supported versions are: {string.Join(", ", supportedNpmVersions)}";

                    _logger.LogError(message);
                    throw new UnsupportedVersionException(message);
                }
            }
            return npmVersion;
        }

        private dynamic GetPackageJsonObject(ISourceRepo sourceRepo)
        {
            dynamic packageJson = null;
            try
            {
                var jsonContent = sourceRepo.ReadFile(PackageJsonFileName);
                packageJson = JsonConvert.DeserializeObject(jsonContent);
            }
            catch
            {
                // we just ignore errors, so we leave malformed package.json
                // files for node.js to handle, not us. This prevents us from
                // erroring out when node itself might be able to tolerate some errors
                // in the package.json file.
            }

            return packageJson;
        }
    }
}