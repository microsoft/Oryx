// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.Detector.Hugo
{
    public class HugoDetector : IHugoPlatformDetector
    {
        private readonly ILogger<HugoDetector> _logger;

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

        public HugoDetector(ILogger<HugoDetector> logger)
        {
            _logger = logger;
        }

        public PlatformDetectorResult Detect(DetectorContext context)
        {
            var isHugoApp = IsHugoApp(context.SourceRepo);
            if (isHugoApp)
            {
                return new PlatformDetectorResult
                {
                    Platform = HugoConstants.PlatformName,
                };
            }

            return null;
        }

        private bool IsHugoApp(ISourceRepo sourceRepo)
        {
            // Hugo configuration variables:
            // https://gohugo.io/getting-started/configuration/#all-configuration-settings

            // Search for config.toml
            if (sourceRepo.FileExists(HugoConstants.TomlFileName)
                && IsHugoTomlFile(sourceRepo, HugoConstants.TomlFileName))
            {
                return true;
            }

            // Search for config.yml
            if (sourceRepo.FileExists(HugoConstants.YmlFileName)
                && IsHugoYamlFile(sourceRepo, HugoConstants.YmlFileName))
            {
                return true;
            }

            // Search for config.yaml
            if (sourceRepo.FileExists(HugoConstants.YamlFileName)
                && IsHugoYamlFile(sourceRepo, HugoConstants.YamlFileName))
            {
                return true;
            }

            // Search for config.json
            if (sourceRepo.FileExists(HugoConstants.JsonFileName)
                && IsHugoYamlFile(sourceRepo, HugoConstants.JsonFileName))
            {
                return true;
            }

            if (sourceRepo.DirExists(HugoConstants.ConfigFolderName))
            {
                // Search for config/**/*.toml
                var tomlFiles = sourceRepo.EnumerateFiles("*.toml", searchSubDirectories: true);
                foreach (var tomlFile in tomlFiles)
                {
                    if (IsHugoTomlFile(sourceRepo, tomlFile))
                    {
                        return true;
                    }
                }

                // Search for config/**/*.yaml and config/**/*.yml
                var yamlFiles = sourceRepo.EnumerateFiles("*.yaml", searchSubDirectories: true);
                foreach (var yamlFile in yamlFiles)
                {
                    if (IsHugoYamlFile(sourceRepo, yamlFile))
                    {
                        return true;
                    }
                }

                var ymlFiles = sourceRepo.EnumerateFiles("*.yml", searchSubDirectories: true);
                foreach (var ymlFile in ymlFiles)
                {
                    if (IsHugoYamlFile(sourceRepo, ymlFile))
                    {
                        return true;
                    }
                }

                // Search for config/**/*.json
                var jsonFiles = sourceRepo.EnumerateFiles("*.json", searchSubDirectories: true);
                foreach (var jsonFile in jsonFiles)
                {
                    if (IsHugoJsonFile(sourceRepo, jsonFile))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsHugoTomlFile(ISourceRepo sourceRepo, params string[] subPaths)
        {
            var tomlTable = ParserHelper.ParseTomlFile(sourceRepo, Path.Combine(subPaths));
            if (tomlTable.Keys
                .Any(k => HugoConfigurationKeys.Contains(k, StringComparer.OrdinalIgnoreCase)))
            {
                return true;
            }

            return false;
        }

        private bool IsHugoYamlFile(ISourceRepo sourceRepo, params string[] subPaths)
        {
            var yamlNode = ParserHelper.ParseYamlFile(sourceRepo, Path.Combine(subPaths));
            if (yamlNode.Children.Keys
                .Select(key => key.ToString())
                .Any(key => HugoConfigurationKeys.Contains(key, StringComparer.OrdinalIgnoreCase)))
            {
                return true;
            }

            return false;
        }

        private bool IsHugoJsonFile(ISourceRepo sourceRepo, params string[] subPaths)
        {
            var jObject = ParserHelper.ParseJsonFile(sourceRepo, Path.Combine(subPaths));
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
