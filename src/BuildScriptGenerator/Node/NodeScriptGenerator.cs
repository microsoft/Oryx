// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    internal class NodeScriptGenerator : ILanguageScriptGenerator
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

        public bool TryGenerateBashScript(ScriptGeneratorContext context, out string script)
        {
            script = null;

            var benvArgs = $"node={context.LanguageVersion} ";

            var npmVersion = GetNpmVersion(context);
            if (!string.IsNullOrEmpty(npmVersion))
            {
                benvArgs += $"npm={npmVersion} ";
            }

            var installCommand = "eval npm install --production";

            script = string.Format(ScriptTemplate, benvArgs, installCommand);

            return true;
        }

        private string GetNpmVersion(ScriptGeneratorContext context)
        {
            var packageJson = GetPackageJsonObject(context.SourceRepo);

            var npmVersionRange = packageJson?.engines?.npm?.Value;
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
                    return null;
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