// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.Common.Extensions;
using Microsoft.Oryx.Detector.Exceptions;
using Newtonsoft.Json.Linq;
using Tomlyn.Model;
using YamlDotNet.RepresentationModel;

namespace Microsoft.Oryx.Detector.Hugo
{
    /// <summary>
    /// An implementation of <see cref="IPlatformDetector"/> which detects Hugo applications.
    /// </summary>
    public class HugoDetector : IHugoPlatformDetector
    {
        internal static readonly string[] HugoConfigurationKeys =
        {
            "archetypeDir",
            "baseURL",
            "contentDir",
            "languageCode",
            "layoutDir",
            "staticDir",
            "title",
            "theme",
        };

        private readonly ILogger<HugoDetector> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HugoDetector"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger{HugoDetector}"/>.</param>
        public HugoDetector(ILogger<HugoDetector> logger)
        {
            this.logger = logger;
        }

        /// <inheritdoc/>
        public PlatformDetectorResult Detect(DetectorContext context)
        {
            var isHugoApp = this.IsHugoApp(context.SourceRepo, out string appDirectory);
            if (isHugoApp)
            {
                return new PlatformDetectorResult
                {
                    Platform = HugoConstants.PlatformName,
                    AppDirectory = appDirectory,
                };
            }

            return null;
        }

        private bool IsHugoApp(ISourceRepo sourceRepo, out string appDirectory)
        {
            // Hugo configuration variables:
            // https://gohugo.io/getting-started/configuration/#all-configuration-settings
            appDirectory = string.Empty;

            // Search for supported .toml file
            foreach (string tomlFileName in HugoConstants.TomlFileNames)
            {
                if (sourceRepo.FileExists(tomlFileName)
                    && this.IsHugoTomlFile(sourceRepo, tomlFileName))
                {
                    return true;
                }
            }

            // Search for supported .yml file
            foreach (string ymlFileName in HugoConstants.YmlFileNames)
            {
                if (sourceRepo.FileExists(ymlFileName)
                    && this.IsHugoYamlFile(sourceRepo, ymlFileName))
                {
                    return true;
                }
            }

            // Search for supported .yaml file
            foreach (string yamlFileName in HugoConstants.YamlFileNames)
            {
                if (sourceRepo.FileExists(yamlFileName)
                    && this.IsHugoYamlFile(sourceRepo, yamlFileName))
                {
                    return true;
                }
            }

            // Search for supported .json file
            foreach (string jsonFileName in HugoConstants.JsonFileNames)
            {
                if (sourceRepo.FileExists(jsonFileName)
                    && this.IsHugoJsonFile(sourceRepo, jsonFileName))
                {
                    return true;
                }
            }

            // NOTE: we do NOT disable looking up into the 'config' folder because it is a special folder
            // from perspective of Hugo where users can have configuration files.
            if (sourceRepo.DirExists(HugoConstants.ConfigFolderName))
            {
                // Search for config/**/*.toml
                var tomlFiles = sourceRepo.EnumerateFiles(
                    "*.toml",
                    searchSubDirectories: true,
                    subDirectoryToSearchUnder: HugoConstants.ConfigFolderName);
                foreach (var tomlFile in tomlFiles)
                {
                    if (this.IsHugoTomlFile(sourceRepo, tomlFile))
                    {
                        return true;
                    }
                }

                // Search for config/**/*.yaml and config/**/*.yml
                var yamlFiles = sourceRepo.EnumerateFiles(
                    "*.yaml",
                    searchSubDirectories: true,
                    subDirectoryToSearchUnder: HugoConstants.ConfigFolderName);
                foreach (var yamlFile in yamlFiles)
                {
                    if (this.IsHugoYamlFile(sourceRepo, yamlFile))
                    {
                        return true;
                    }
                }

                var ymlFiles = sourceRepo.EnumerateFiles(
                    "*.yml",
                    searchSubDirectories: true,
                    subDirectoryToSearchUnder: HugoConstants.ConfigFolderName);
                foreach (var ymlFile in ymlFiles)
                {
                    if (this.IsHugoYamlFile(sourceRepo, ymlFile))
                    {
                        return true;
                    }
                }

                // Search for config/**/*.json
                var jsonFiles = sourceRepo.EnumerateFiles(
                    "*.json",
                    searchSubDirectories: true,
                    subDirectoryToSearchUnder: HugoConstants.ConfigFolderName);
                foreach (var jsonFile in jsonFiles)
                {
                    if (this.IsHugoJsonFile(sourceRepo, jsonFile))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsHugoTomlFile(ISourceRepo sourceRepo, params string[] subPaths)
        {
            TomlTable tomlTable = null;
            var relativeFilePath = Path.Combine(subPaths);

            try
            {
                tomlTable = ParserHelper.ParseTomlFile(sourceRepo, relativeFilePath);
            }
            catch (FailedToParseFileException ex)
            {
                this.logger.LogError(ex, $"An error occurred when trying to parse file '{relativeFilePath.Hash()}'.");
                return false;
            }

            if (tomlTable.Keys
                .Any(k => HugoConfigurationKeys.Contains(k, StringComparer.OrdinalIgnoreCase)))
            {
                return true;
            }

            return false;
        }

        private bool IsHugoYamlFile(ISourceRepo sourceRepo, params string[] subPaths)
        {
            var relativeFilePath = Path.Combine(subPaths);

            YamlNode yamlNode = null;

            try
            {
                yamlNode = ParserHelper.ParseYamlFile(sourceRepo, relativeFilePath);
            }
            catch (FailedToParseFileException ex)
            {
                this.logger.LogError(ex, $"An error occurred when trying to parse file '{relativeFilePath.Hash()}'.");
                return false;
            }

            var yamlMappingNode = yamlNode as YamlMappingNode;
            if (yamlMappingNode != null)
            {
                if (yamlMappingNode.Children.Keys
                    .Select(key => key.ToString())
                    .Any(key => HugoConfigurationKeys.Contains(key, StringComparer.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsHugoJsonFile(ISourceRepo sourceRepo, params string[] subPaths)
        {
            var relativeFilePath = Path.Combine(subPaths);

            JObject jObject = null;

            try
            {
                jObject = ParserHelper.ParseJsonFile(sourceRepo, relativeFilePath);
            }
            catch (FailedToParseFileException ex)
            {
                this.logger.LogError(ex, $"An error occurred when trying to parse file '{relativeFilePath.Hash()}'.");
                return false;
            }

            if (jObject.Children()
                .Select(c => c.Path)
                .Any(c => HugoConfigurationKeys.Contains(c, StringComparer.OrdinalIgnoreCase)))
            {
                return true;
            }

            return false;
        }
    }
}
