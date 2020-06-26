// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.Detector.Hugo
{
    public class HugoDetector : IPlatformDetector
    {
        private readonly ILogger<HugoDetector> _logger;

        private readonly string[] HugoConfigurationKeys =
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

        public PlatformName PlatformName => PlatformName.Hugo;

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
                    .Any(k => HugoConfigurationKeys.Contains(k, StringComparer.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            foreach (var path in yamlFilePaths)
            {
                var yamlNode = ParserHelper.ParseYamlFile(sourceRepo, path);
                if (yamlNode.Children.Keys
                    .Select(k => k.ToString())
                    .Any(k => HugoConfigurationKeys.Contains(k, StringComparer.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            foreach (var path in jsonFilePaths)
            {
                var jObject = ParserHelper.ParseJsonFile(sourceRepo, path);
                if (jObject.Children()
                    .Select(c => c.Path)
                    .Any(c => HugoConfigurationKeys.Contains(c, StringComparer.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
