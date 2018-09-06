// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    using System;
    using System.IO;
    using Microsoft.Oryx.BuildScriptGenerator;
    using Microsoft.Oryx.BuildScriptGenerator.SourceRepo;

    internal class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                var options = new Options(args);
                var sourceFolder = Path.GetFullPath(options.SourceCodeFolder);
                if (!Directory.Exists(sourceFolder))
                {
                    Console.WriteLine($"Couldn't find directory {sourceFolder}");
                    return 1;
                }

                var sourceRepo = new LocalSourceRepo(sourceFolder);
                IBuildScriptBuilder scriptBuilder;
                var langDetector = new LanguageDetector();

                if (string.IsNullOrEmpty(options.Language))
                {
                    scriptBuilder = langDetector.GetBuildScriptBuilder(sourceRepo);
                    if (scriptBuilder == null)
                    {
                        Console.WriteLine($"The provided language '{options.Language}' doesn't match what we found in '{sourceFolder}.");
                        return 1;
                    }
                }
                else
                {
                    scriptBuilder = langDetector.GetBuildScriptBuilder(options.Language, sourceRepo);
                    if (scriptBuilder == null)
                    {
                        Console.WriteLine($"Couldn't detect the language used in {sourceFolder}");
                        return 1;
                    }
                }

                var scriptContent = scriptBuilder.GenerateShScript();
                var targetScriptPath = Path.GetFullPath(options.TargetScriptPath);
                File.WriteAllText(targetScriptPath, scriptContent);
            }
            catch (InvalidUsageException ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
            catch
            {
                Console.WriteLine("Oops... An unexpected error has occurred.");
                return 1;
            }
            return 0;
        }
    }
}