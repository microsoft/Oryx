// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    internal class PythonScriptGenerator : IScriptGenerator
    {
        private const string PythonName = "python";
        private const string RequirementsFileName = "requirements.txt";


        private readonly PythonScriptGeneratorOptions _pythonScriptGeneratorOptions;
        private readonly IPythonVersionProvider _pythonVersionProvider;
        private IPythonVersionResolver _versionResolver;
        private readonly ILogger<PythonScriptGenerator> _logger;

        private const string ScriptTemplate =
            @"#!/bin/bash
SOURCE_DIR=$1
OUTPUT_DIR=$2

source /usr/local/bin/benv {0}

echo Python deployment.

#1. Install any dependencies
{1}

echo ""$SOURCE_DIR""
echo ""$OUTPUT_DIR""

echo ""Found requirements.txt""
echo ""Python Virtual Environment: $ANTENV""
echo ""Python Version: $python""

cd ""$SOURCE_DIR""
    
cp -rf . ""$OUTPUT_DIR""

cd ""$OUTPUT_DIR""

#2a. Setup virtual Environment
echo ""Create virtual environment""
$python -m venv $ANTENV --copies

#2b. Activate virtual environment
echo ""Activate virtual environment""
source $ANTENV/bin/activate

#2c. Install dependencies
pip install -r requirements.txt

echo ""pip install finished""
";

        public PythonScriptGenerator(
            IOptions<PythonScriptGeneratorOptions> pythonScriptGeneratorOptions,
            IPythonVersionProvider pythonVersionProvider,
            IPythonVersionResolver pythonVersionResolver,
            ILogger<PythonScriptGenerator> logger)
        {
            _pythonScriptGeneratorOptions = pythonScriptGeneratorOptions.Value;
            _pythonVersionProvider = pythonVersionProvider;
            _versionResolver = pythonVersionResolver;
            _logger = logger;
        }

        public string SupportedLanguageName => PythonName;

        public IEnumerable<string> SupportedLanguageNames => new[] { PythonName };

        public IEnumerable<string> SupportedLanguageVersions => _pythonVersionProvider.SupportedPythonVersions;

        public bool CanGenerateScript(ScriptGeneratorContext context)
        {
            if (context.SourceRepo.FileExists(RequirementsFileName))
            {
                return true;
            }
            return false;
        }

        public string GenerateBashScript(ScriptGeneratorContext context)
        {
            var pythonVersion = context.LanguageVersion;

            var benvArgs = string.IsNullOrEmpty(pythonVersion) ? string.Empty : $"python={pythonVersion} ";
            var antenvCommand = "3.6.6".Equals(context.LanguageVersion)
                ? "export ANTENV=\"antenv3.6\""
                : "export ANTENV=\"antenv\"";
            var scriptWithLF = ScriptTemplate.Replace("\r\n", "\n");
            return string.Format(scriptWithLF, benvArgs, antenvCommand);
        }
    }
}