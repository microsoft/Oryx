// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    using System.Collections.Generic;
    using System.Text;
    using BuildScriptGenerator.Exceptions;
    using BuildScriptGenerator.SourceRepo;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;

    internal class NodeScriptGenerator : IScriptGenerator
    {
        private const string NodeJsName = "nodejs";
        private const string PackageFileName = "package.json";

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
        private readonly ILogger<NodeScriptGenerator> _logger;
        private readonly NodeScriptGeneratorOptions _options;
        private INodeVersionResolver _versionResolver;

        public NodeScriptGenerator(
            IOptions<NodeScriptGeneratorOptions> nodeScriptGeneratorOptions,
            INodeVersionResolver nodeVersionResolver,
            ILogger<NodeScriptGenerator> logger)
        {
            _options = nodeScriptGeneratorOptions.Value;
            _versionResolver = nodeVersionResolver;
            _logger = logger;
        }

        public string LanguageName => NodeJsName;

        public IEnumerable<string> LanguageVersions => _options.SupportedNodeVersions;

        public bool CanGenerateScript(ISourceRepo sourceRepo, string language)
        {
            var unsupportedProvidedLanguage = !string.IsNullOrWhiteSpace(language) &&
                string.Compare(NodeJsName, language, ignoreCase: true) != 0;

            if (unsupportedProvidedLanguage)
            {
                _logger.LogInformation(
                    $"Does not support the language with name '{language}'." +
                    $"Supported language name is '{LanguageName}'.");
                return false;
            }

            if (sourceRepo.FileExists(PackageFileName))
            {
                return true;
            }

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
                        return false;
                    }
                }
                return true;
            }

            return false;
        }

        public string GenerateBashScript(ISourceRepo sourceRepo)
        {
            var scriptBuilder = new StringBuilder();

            var packageJson = GetPackageJsonObject(sourceRepo);
            var nodeVersion = DetectNodeVersion(packageJson);
            var npmVersion = DetectNpmVersion(packageJson);

            scriptBuilder.AppendLine("#!/bin/bash");

            scriptBuilder.Append("source /usr/local/bin/benv ");
            if (!string.IsNullOrEmpty(nodeVersion))
            {
                scriptBuilder.Append($"node={nodeVersion} ");
            }
            if (!string.IsNullOrEmpty(npmVersion))
            {
                scriptBuilder.Append($"npm={npmVersion} ");
            }
            scriptBuilder.AppendLine();

            scriptBuilder.AppendLine("# Install npm packages");
            scriptBuilder.AppendLine($"cd \"{sourceRepo.RootPath}\"");
            var installCommand = "eval npm install --production";
            scriptBuilder.AppendLine($"echo \"Running {installCommand}\"");
            scriptBuilder.AppendLine(installCommand);

            return scriptBuilder.ToString();
        }

        private string DetectNodeVersion(dynamic packageJson)
        {
            var nodeVersionRange = packageJson?.engines?.node?.Value as string;
            if (nodeVersionRange == null)
            {
                nodeVersionRange = _options.NodeJsDefaultVersion;
            }
            string nodeVersion = null;
            if (!string.IsNullOrWhiteSpace(nodeVersionRange))
            {
                nodeVersion = _versionResolver.GetSupportedNodeVersion(nodeVersionRange);
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
                npmVersionRange = _options.NpmDefaultVersion;
            }
            string npmVersion = null;
            if (!string.IsNullOrWhiteSpace(npmVersionRange))
            {
                npmVersion = _versionResolver.GetSupportedNpmVersion(npmVersionRange);
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
                var jsonContent = sourceRepo.ReadFile("package.json");
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