// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    using System.Collections.Generic;
    using BuildScriptGenerator.SourceRepo;

    internal class NodeLanguage : ILanguage
    {
        private const string PackageFileName = "package.json";
        private static readonly string[] TypicalNodeDetectionFiles = new[] { "server.js", "app.js" };
        private static readonly string[] LanguageNames = new[] { "node", "nodejs" };

        private static readonly string[] IisStartupFiles = new[]
        {
            "default.htm", "default.html", "default.asp", "index.htm", "index.html", "iisstart.htm", "default.aspx", "index.php"
        };

        public IEnumerable<string> Name => LanguageNames;

        public bool TryGetBuildScriptBuilder(ISourceRepo sourceRepo, out IBuildScriptBuilder buildScriptBuilder)
        {
            var isNodeSource = IsUsedIn(sourceRepo);
            if (isNodeSource)
            {
                buildScriptBuilder = new NodeScriptBuilder(sourceRepo, new NodeEnvironmentVariableSettings());
            }
            else
            {
                buildScriptBuilder = null;
            }
            return isNodeSource;
        }

        private bool IsUsedIn(ISourceRepo sourceRepo)
        {
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
    }
}