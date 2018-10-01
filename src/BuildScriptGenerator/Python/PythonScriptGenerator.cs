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

        private const string DefaultPythonVersion = "3.7.0"; 

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
            if (!context.SourceRepo.FileExists(RequirementsFileName))
            {
                _logger.LogDebug($"Cannot generate script as source folder does not have file '{RequirementsFileName}'.");
                return false;
            }
            var sourceFolder = context.SourceRepo.RootPath;
            var pythonFiles = Directory.GetFileSystemEntries(sourceFolder, PythonFileExtension);
            if (pythonFiles.Length > 0)
            {
                return true;
            }

            // Most Python sites will have at least a .py file in the root, but
            // some may not. In that case, let them opt in with the runtime.txt
            // file, which is used to specify the version of Python.
            if (context.SourceRepo.FileExists(RuntimeFileName))
            {
                try
                {
                    var text = context.SourceRepo.ReadFile(RuntimeFileName);
                    return text.IndexOf("python", StringComparison.OrdinalIgnoreCase) >= 0;
                }
                catch (IOException)
                {
                    return false;
                }
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
            var scriptWithLF = ScriptTemplate.Replace("\r\n", "\n");
            return string.Format(scriptWithLF, benvArgs, antenvCommand);
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
                pythonVersion = _versionResolver.GetSupportedPythonVersion(pythonVersionRange);
                if (string.IsNullOrWhiteSpace(pythonVersion))
                {
                    throw new UnsupportedPythonVersionException(pythonVersionRange);
                }
            }
            return pythonVersion;
        }
    }
}