// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    using System.Linq;
    using System.Text;
    using BuildScriptGenerator.SourceRepo;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;

    internal class NodeScriptGenerator : IScriptGenerator
    {
        private INodeVersionResolver _versionResolver;
        private readonly ILogger<NodeScriptGenerator> _logger;
        private ISourceRepo _sourceRepo;
        private readonly BuildScriptGeneratorOptions _buildScriptGeneratorOptions;
        private readonly NodeScriptGeneratorOptions _options;
        private dynamic _packageJson;

        private const string PackageFileName = "package.json";
        private static readonly string[] TypicalNodeDetectionFiles = new[] { "server.js", "app.js" };
        private static readonly string[] LanguageNames = new[] { "node", "nodejs" };
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

        public NodeScriptGenerator(
            ISourceRepo sourceRepo,
            IOptions<BuildScriptGeneratorOptions> buildScriptGeneratorOptions,
            IOptions<NodeScriptGeneratorOptions> nodeScriptGeneratorOptions,
            INodeVersionResolver nodeVersionResolver,
            ILogger<NodeScriptGenerator> logger)
        {
            _sourceRepo = sourceRepo;
            _buildScriptGeneratorOptions = buildScriptGeneratorOptions.Value;
            _options = nodeScriptGeneratorOptions.Value;
            _versionResolver = nodeVersionResolver;
            _logger = logger;
        }

        public bool CanGenerateShScript()
        {
            if (!IsSupportedLanguage())
            {
                _logger.LogDebug(
                    $"Does not support the language with name '{_buildScriptGeneratorOptions.Language}'." +
                    $"Supported language names are: {string.Join(", ", LanguageNames)}");
                return false;
            }

            if (_sourceRepo.FileExists(PackageFileName))
            {
                return true;
            }

            // Copying the logic currently running in Kudu:
            var mightBeNode = false;
            foreach (var typicalNodeFile in TypicalNodeDetectionFiles)
            {
                if (_sourceRepo.FileExists(typicalNodeFile))
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
                    if (_sourceRepo.FileExists(iisStartupFile))
                    {
                        return false;
                    }
                }
                return true;
            }

            return false;
        }

        public string GenerateShScript()
        {
            var scriptBuilder = new StringBuilder();

            var nodeVersion = DetectNodeVersion();
            var npmVersion = DetectNpmVersion();

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
            scriptBuilder.AppendLine($"cd \"{_sourceRepo.RootPath}\"");
            var installCommand = "eval npm install --production";
            scriptBuilder.AppendLine($"echo \"Running {installCommand}\"");
            scriptBuilder.AppendLine(installCommand);

            return scriptBuilder.ToString();
        }

        private bool IsSupportedLanguage()
        {
            if (string.IsNullOrEmpty(_buildScriptGeneratorOptions.Language))
            {
                return true;
            }
            else
            {
                var isSupportedLanguage = LanguageNames.Any(
                    lang => string.Compare(lang, _buildScriptGeneratorOptions.Language, ignoreCase: true) == 0);
                return isSupportedLanguage;
            }
        }

        private dynamic GetPackageJsonObject()
        {
            if (_packageJson == null)
            {
                try
                {
                    var jsonContent = _sourceRepo.ReadFile("package.json");
                    _packageJson = JsonConvert.DeserializeObject(jsonContent);
                }
                catch
                {
                    // we just ignore errors, so we leave malformed package.json
                    // files for node.js to handle, not us. This prevents us from
                    // erroring out when node itself might be able to tolerate some errors
                    // in the package.json file.
                }
            }

            return _packageJson;
        }

        private string DetectNodeVersion()
        {
            var packageJson = GetPackageJsonObject();
            var nodeVersionRange = packageJson?.engines?.node?.Value as string;
            if (nodeVersionRange == null)
            {
                nodeVersionRange = _options.NodeJsDefaultVersion;
            }
            string nodeVersion = null;
            if (!string.IsNullOrWhiteSpace(nodeVersionRange))
            {
                nodeVersion = _versionResolver.GetSupportedNodeVersion(nodeVersionRange);
            }
            return nodeVersion;
        }

        private string DetectNpmVersion()
        {
            var packageJson = GetPackageJsonObject();
            string npmVersionRange = packageJson?.engines?.npm?.Value;
            if (npmVersionRange == null)
            {
                npmVersionRange = _options.NpmDefaultVersion;
            }
            string npmVersion = null;
            if (!string.IsNullOrWhiteSpace(npmVersionRange))
            {
                npmVersion = _versionResolver.GetSupportedNpmVersion(npmVersionRange);
            }
            return npmVersion;
        }
    }
}