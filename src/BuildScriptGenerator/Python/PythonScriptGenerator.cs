// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    internal class PythonScriptGenerator : ILanguageScriptGenerator
    {
        private const string PythonName = "python";
        private const string DefaultVirtualEnvironmentName = "pythonenv";
        private const string VirtualEnvironmentNamePropertyKey = "virtualenv_name";

        private readonly PythonScriptGeneratorOptions _pythonScriptGeneratorOptions;
        private readonly IPythonVersionProvider _pythonVersionProvider;
        private readonly ILogger<PythonScriptGenerator> _logger;

        private const string ScriptTemplate =
            @"#!/bin/bash
set -e

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

echo ""Source directory     : $SOURCE_DIR""
echo ""Destination directory: $DESTINATION_DIR""

source /usr/local/bin/benv {0}

VIRTUALENVIRONMENTNAME={1}
VIRTUALENVIRONMENTMODULE={2}
VIRTUALENVCOPYPARAMETER={3}

echo ""Python Virtual Environment: $VIRTUALENVIRONMENTNAME""
echo ""Python Version: $python""

cd ""$SOURCE_DIR""

echo Creating virtual environment ...
$python -m $VIRTUALENVIRONMENTMODULE $VIRTUALENVIRONMENTNAME $VIRTUALENVCOPYPARAMETER

echo Activating virtual environment ...
source $VIRTUALENVIRONMENTNAME/bin/activate

echo Pip Version:
$pip --version

$pip install -r requirements.txt

echo
echo pip install finished.

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

        public IEnumerable<string> SupportedLanguageVersions => _pythonVersionProvider.SupportedPythonVersions;

        public bool TryGenerateBashScript(ScriptGeneratorContext context, out string script)
        {
            if (context.Properties == null ||
                !context.Properties.TryGetValue(VirtualEnvironmentNamePropertyKey, out var virtualEnvName))
            {
                virtualEnvName = DefaultVirtualEnvironmentName;
            }

            var virtualEnvModule = "venv";
            var virtualEnvCopyParam = string.Empty;

            var pythonVersion = context.LanguageVersion;
            if (!string.IsNullOrEmpty(pythonVersion))
            {
                switch (pythonVersion.Substring(0, 1))
                {
                    case "2":
                        virtualEnvModule = "virtualenv";
                        break;
                    case "3":
                        virtualEnvModule = "venv";
                        virtualEnvCopyParam = "--copies";
                        break;
                    default:
                        string errorMessage = "Python version " + pythonVersion + " is not supported";
                        _logger.LogError(errorMessage);
                        throw new NotSupportedException(errorMessage);
                }
            }

            script = string.Format(ScriptTemplate, $"python={pythonVersion}", virtualEnvName, virtualEnvModule, virtualEnvCopyParam);

            return true;
        }
    }
}