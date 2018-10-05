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
OUTPUT_DIR=$2

if [ ! $# -eq 2 ]; then
    echo ""Usage: $0 <source-dir> <output-dir>""
    exit 1
fi

if [ ! -d ""$SOURCE_DIR"" ]; then
    echo ""Source directory '$SOURCE_DIR' does not exist.""
    exit 1
fi

if [ -z ""$OUTPUT_DIR"" ]; then
    echo ""Output directory is required.""
    exit 1
fi

source /usr/local/bin/benv {0}

echo Installing npm packages ...
cd ""$SOURCE_DIR""
echo
echo ""Running '{1}' ...""
echo
{1}

if [ -d ""$OUTPUT_DIR"" ]
then
    echo
    echo Output directory already exists. Deleting it ...
    rm -rf ""$OUTPUT_DIR""
fi

echo
echo Creating output directory ...
mkdir -p ""$OUTPUT_DIR""

echo
echo ""Copying output from '$SOURCE_DIR' to '$OUTPUT_DIR' ...""
cp -r . ""$OUTPUT_DIR""

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

            // C# multiline string literals always seem to be represented as CRLF, so replace that line
            // ending with LF
            var scriptTemplateWithLF = ScriptTemplate.Replace("\r\n", "\n");
            var script = string.Format(scriptTemplateWithLF, benvArgs, installCommand);
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
                    throw new UnsupportedNodeVersionException(nodeVersionRange);
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
                npmVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(
                    npmVersionRange,
                    _nodeVersionProvider.SupportedNpmVersions);
                if (string.IsNullOrWhiteSpace(npmVersion))
                {
                    throw new UnsupportedNpmVersionException(npmVersionRange);
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