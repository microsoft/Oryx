// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    internal class PythonScriptGenerator : IScriptGenerator
    {
        private const string PythonName = "python";
        private const string RequirementsFileName = "requirements.txt";
        private const string RuntimeFileName = "runtime.txt";
        private const string PythonFileExtension = "*.py";

        private readonly PythonScriptGeneratorOptions _pythonScriptGeneratorOptions;
        private readonly IPythonVersionProvider _pythonVersionProvider;
        private readonly ILogger<PythonScriptGenerator> _logger;

        private const string ScriptTemplate =
            @"#!/bin/bash
set -e

SOURCE_DIR=$1
DESTINATION_DIR=$2
TEMP_ROOT_DIR=$3
FORCE=$4

if [ ! -d ""$SOURCE_DIR"" ]; then
    echo ""Source directory '$SOURCE_DIR' does not exist."" 1>&2
    exit 1
fi

if [ ! -d ""$TEMP_ROOT_DIR"" ]; then
    echo ""Temp root directory '$TEMP_ROOT_DIR' does not exist."" 1>&2
    exit 1
fi

# Get full file paths to source and destination directories
cd $SOURCE_DIR
SOURCE_DIR=$(pwd -P)

if [ -d ""$DESTINATION_DIR"" ]
then
    cd $DESTINATION_DIR
    DESTINATION_DIR=$(pwd -P)
fi

if [ ! ""$SOURCE_DIR"" == ""$DESTINATION_DIR"" ]
then
    if [ -d ""$DESTINATION_DIR"" ]
    then
        if [ ! ""$FORCE"" == ""True"" ]
        then
            echo ""Destination directory is not empty. Use the '-f' or '--force' option to replace the content."" 1>&2
            exit 1
        fi
    fi
fi

echo
echo ""Source directory     : $SOURCE_DIR""
echo ""Destination directory: $DESTINATION_DIR""
echo

source /usr/local/bin/benv {0}

echo Python deployment.

#1. Install any dependencies
{1}

echo ""Python Virtual Environment: $ANTENV""
echo ""Python Version: $python""

cd ""$SOURCE_DIR""

#2a. Setup virtual Environment
echo ""Create virtual environment""
$python -m venv $ANTENV --copies

#2b. Activate virtual environment
echo ""Activate virtual environment""
source $ANTENV/bin/activate

#2c. Install dependencies
pip install -r requirements.txt

echo
echo ""pip install finished""

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

appTempDir=""$TEMP_ROOT_DIR/output""
cp -rf ""$SOURCE_DIR"" ""$appTempDir""
mkdir -p ""$DESTINATION_DIR""
cp -rf ""$appTempDir""/* ""$DESTINATION_DIR""

echo
echo Done.
";

        private const string DefaultPythonVersion = "3.7.0";

        public PythonScriptGenerator(
            IOptions<PythonScriptGeneratorOptions> pythonScriptGeneratorOptions,
            IPythonVersionProvider pythonVersionProvider,
            ILogger<PythonScriptGenerator> logger)
        {
            _pythonScriptGeneratorOptions = pythonScriptGeneratorOptions.Value;
            _pythonVersionProvider = pythonVersionProvider;
            _logger = logger;
        }

        public string SupportedLanguageName => PythonName;

        public IEnumerable<string> SupportedLanguageNames => new[] { PythonName };

        public IEnumerable<string> SupportedLanguageVersions => _pythonVersionProvider.SupportedPythonVersions;

        public bool CanGenerateScript(ScriptGeneratorContext context)
        {
            if (!context.SourceRepo.FileExists(RequirementsFileName))
            {
                _logger.LogDebug(
                    $"Cannot generate script as source directory does not have file '{RequirementsFileName}'.");
                return false;
            }

            var sourceDir = context.SourceRepo.RootPath;
            var pythonFiles = Directory.GetFileSystemEntries(sourceDir, PythonFileExtension);
            if (pythonFiles.Length > 0)
            {
                return true;
            }
            else
            {
                _logger.LogDebug(
                    $"Could not find any files with file extension '{PythonFileExtension}' in source directory.");
            }

            // Most Python sites will have at least a .py file in the root, but
            // some may not. In that case, let them opt in with the runtime.txt
            // file, which is used to specify the version of Python.
            if (context.SourceRepo.FileExists(RuntimeFileName))
            {
                try
                {
                    var text = context.SourceRepo.ReadFile(RuntimeFileName);
                    var hasPythonVersion = text.IndexOf("python", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (!hasPythonVersion)
                    {
                        _logger.LogDebug(
                            $"Cound not find any text of the form 'python=' in the file '{RuntimeFileName}'.");
                        return false;
                    }
                    return true;
                }
                catch (IOException ex)
                {
                    _logger.LogError(
                        $"An error occurred while trying to read the file '{RuntimeFileName}'. Exception: {ex}");
                    return false;
                }
            }
            else
            {
                _logger.LogDebug($"Could not find file '{RuntimeFileName}' in source directory.");
            }

            return false;
        }

        public string GenerateBashScript(ScriptGeneratorContext context)
        {
            var pythonVersion = DetectPythonVersion(context);

            var benvArgs = string.IsNullOrEmpty(pythonVersion) ? string.Empty : $"python={pythonVersion} ";
            var antenvCommand = "3.6.6".Equals(context.LanguageVersion)
                ? "export ANTENV=\"antenv3.6\""
                : "export ANTENV=\"antenv\"";
            return string.Format(ScriptTemplate, benvArgs, antenvCommand);
        }

        private string DetectPythonVersion(ScriptGeneratorContext context)
        {
            string pythonVersionRange = null;
            string pythonVersion = null;
            if (context.SourceRepo.FileExists(RuntimeFileName))
            {
                // Check runtime.txt for python version; if not specified, check the context.
                // If not present, use default version specified by environment variable. If null, use 3.7.0.
                try
                {
                    var text = context.SourceRepo.ReadFile(RuntimeFileName);
                    pythonVersionRange = text.Remove(0, "python-".Length);
                }
                catch (IOException)
                {
                }
            }
            if (pythonVersionRange == null)
            {
                pythonVersionRange = (context.LanguageVersion == null ? _pythonScriptGeneratorOptions.PythonDefaultVersion : context.LanguageVersion) ??
                                     DefaultPythonVersion;
            }
            if (!string.IsNullOrWhiteSpace(pythonVersionRange))
            {
                pythonVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(
                    pythonVersionRange,
                    SupportedLanguageVersions);
                if (string.IsNullOrWhiteSpace(pythonVersion))
                {
                    var message = $"The target Python version '{pythonVersionRange}' is not supported. " +
                        $"Supported versions are: {string.Join(", ", SupportedLanguageVersions)}";

                    _logger.LogError(message);
                    throw new UnsupportedVersionException(message);
                }
            }
            return pythonVersion;
        }
    }
}