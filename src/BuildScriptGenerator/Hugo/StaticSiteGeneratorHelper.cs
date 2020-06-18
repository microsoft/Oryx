// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Oryx.BuildScriptGenerator.Hugo
{
    /// <summary>
    /// Helper class for functions around static site generators.
    /// </summary>
    internal static class StaticSiteGeneratorHelper
    {
        private static string HugoEnvironmentVariablePrefix = "HUGO_";
        private static string[] HugoConfigurationVariables =
            { "archetypeDir", "baseURL", "contentDir", "languageCode", "layoutDir", "staticDir", "title", "theme" };

        /// <summary>
        /// Checks whether or not the given repository uses a static site generator.
        /// </summary>
        /// <param name="sourceRepo">Source repo for the application.</param>
        /// <param name="environment">Environment abstraction.</param>
        /// <returns>True if the app uses a static site generator, false otherwise.</returns>
        public static bool IsStaticSite(ISourceRepo sourceRepo, IEnvironment environment)
        {
            return IsHugoApp(sourceRepo, environment);
        }

        /// <summary>
        /// Checks whether or not the given repository is a Hugo application.
        /// </summary>
        /// <param name="sourceRepo">Source repo for the application.</param>
        /// <param name="environment">Environment abstraction.</param>
        /// <returns>True if the app is a Hugo app, false otherwise.</returns>
        public static bool IsHugoApp(ISourceRepo sourceRepo, IEnvironment environment)
        {
            // Check for Hugo environment variables
            var environmentVariables = environment.GetEnvironmentVariables();
            foreach (var key in environmentVariables?.Keys)
            {
                if (key.ToString().StartsWith(HugoEnvironmentVariablePrefix))
                {
                    return true;
                }
            }

            // Hugo configuration variables: https://gohugo.io/getting-started/configuration/#all-configuration-settings
            var tomlFilePaths = new List<string>();
            var yamlFilePaths = new List<string>();
            var jsonFilePaths = new List<string>();

            // Search for config.toml
            if (sourceRepo.FileExists(HugoConstants.TomlFileName))
            {
                tomlFilePaths.Add(Path.Combine(sourceRepo.RootPath, HugoConstants.TomlFileName));
            }

            // Search for config.yml
            if (sourceRepo.FileExists(HugoConstants.YmlFileName))
            {
                yamlFilePaths.Add(Path.Combine(sourceRepo.RootPath, HugoConstants.YmlFileName));
            }

            // Search for config.yaml
            if (sourceRepo.FileExists(HugoConstants.YamlFileName))
            {
                yamlFilePaths.Add(Path.Combine(sourceRepo.RootPath, HugoConstants.YamlFileName));
            }

            // Search for config.json
            if (sourceRepo.FileExists(HugoConstants.JsonFileName))
            {
                jsonFilePaths.Add(Path.Combine(sourceRepo.RootPath, HugoConstants.JsonFileName));
            }

            if (sourceRepo.DirExists(HugoConstants.ConfigFolderName))
            {
                // Search for config/*.toml
                tomlFilePaths.AddRange(sourceRepo.EnumerateFiles("*.toml", searchSubDirectories: true));

                // Search for config/*.yaml and config/*.yml
                yamlFilePaths.AddRange(sourceRepo.EnumerateFiles("*.yaml", searchSubDirectories: true));
                yamlFilePaths.AddRange(sourceRepo.EnumerateFiles("*.yml", searchSubDirectories: true));

                // Search for config/*.json
                jsonFilePaths.AddRange(sourceRepo.EnumerateFiles("*.json", searchSubDirectories: true));
            }

            foreach (var path in tomlFilePaths)
            {
                var tomlTable = ParserHelper.ParseTomlFile(sourceRepo, path);
                if (tomlTable.Keys
                    .Any(k => HugoConfigurationVariables.Contains(k, StringComparer.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            foreach (var path in yamlFilePaths)
            {
                var yamlNode = ParserHelper.ParseYamlFile(sourceRepo, path);
                if (yamlNode.Children.Keys
                    .Select(k => k.ToString())
                    .Any(k => HugoConfigurationVariables.Contains(k, StringComparer.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            foreach (var path in jsonFilePaths)
            {
                var jObject = ParserHelper.ParseJsonFile(sourceRepo, path);
                if (jObject.Children()
                    .Select(c => c.Path)
                    .Any(c => HugoConfigurationVariables.Contains(c, StringComparer.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
