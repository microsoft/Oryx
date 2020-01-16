// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Oryx.BuildScriptGenerator.Node;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Helper class for functions around static site generators.
    /// </summary>
    internal static class StaticSiteGeneratorHelper
    {
        /// <summary>
        /// Checks whether or not the given repository uses a static site generator.
        /// </summary>
        /// <param name="sourceRepo">Source repo for the application.</param>
        /// <returns>True if the app uses a static site generator, false otherwise.</returns>
        public static bool IsStaticSite(ISourceRepo sourceRepo)
        {
            return IsHugoApp(sourceRepo);
        }

        /// <summary>
        /// Checks whether or not the given repository is a Hugo application.
        /// </summary>
        /// <param name="sourceRepo">Source repo for the application.</param>
        /// <returns>True if the app is a Hugo app, false otherwise.</returns>
        public static bool IsHugoApp(ISourceRepo sourceRepo)
        {
            // Check for Hugo environment variables
            var environmentVariables = Environment.GetEnvironmentVariables();
            foreach (var key in environmentVariables?.Keys)
            {
                if (key.ToString().StartsWith("HUGO"))
                {
                    return true;
                }
            }

            // Hugo configuration variables: https://gohugo.io/getting-started/configuration/#all-configuration-settings
            var hugoVariables = new[] { "baseURL", "title", "languageCode", "theme" };
            var tomlFilePaths = new List<string>();
            var yamlFilePaths = new List<string>();
            var jsonFilePaths = new List<string>();

            // Search for config.toml
            if (sourceRepo.FileExists(NodeConstants.HugoTomlFileName))
            {
                tomlFilePaths.Add(Path.Combine(sourceRepo.RootPath, NodeConstants.HugoTomlFileName));
            }

            // Search for config.yaml
            if (sourceRepo.FileExists(NodeConstants.HugoYamlFileName))
            {
                yamlFilePaths.Add(Path.Combine(sourceRepo.RootPath, NodeConstants.HugoYamlFileName));
            }

            // Search for config.json
            if (sourceRepo.FileExists(NodeConstants.HugoJsonFileName))
            {
                jsonFilePaths.Add(Path.Combine(sourceRepo.RootPath, NodeConstants.HugoJsonFileName));
            }

            if (sourceRepo.DirExists(NodeConstants.HugoConfigFolderName))
            {
                var configSourceRepo = new LocalSourceRepo(Path.Combine(sourceRepo.RootPath, NodeConstants.HugoConfigFolderName));

                // Search for config/*.toml
                tomlFilePaths.AddRange(configSourceRepo.EnumerateFiles("*.toml", true));

                // Search for config/*.yaml
                yamlFilePaths.AddRange(configSourceRepo.EnumerateFiles("*.yaml", true));

                // Search for config/*.json
                jsonFilePaths.AddRange(configSourceRepo.EnumerateFiles("*.json", true));
            }

            foreach (var path in tomlFilePaths)
            {
                var tomlTable = ParserHelper.ParseTomlFile(sourceRepo, path);
                if (tomlTable.Keys.Any(k => hugoVariables.Contains(k)))
                {
                    return true;
                }
            }

            foreach (var path in yamlFilePaths)
            {
                var yamlNode = ParserHelper.ParseYamlFile(sourceRepo, path);
                if (yamlNode.Children.Keys.Select(k => k.ToString()).Any(k => hugoVariables.Contains(k)))
                {
                    return true;
                }
            }

            foreach (var path in jsonFilePaths)
            {
                var jObject = ParserHelper.ParseJsonFile(sourceRepo, path);
                if (jObject.Children().Select(c => c.Path).Any(c => hugoVariables.Contains(c)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
