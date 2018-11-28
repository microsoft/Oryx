// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal class DefaultEnvironmentSettingsProvider : IEnvironmentSettingsProvider
    {
        private readonly ISourceRepo _sourceRepo;
        private readonly IConsole _console;
        private readonly ILogger<DefaultEnvironmentSettingsProvider> _logger;
        private Dictionary<string, string> _settings;

        public DefaultEnvironmentSettingsProvider(
            ISourceRepoProvider sourceRepoProvider,
            IConsole console,
            ILogger<DefaultEnvironmentSettingsProvider> logger)
        {
            _sourceRepo = sourceRepoProvider.GetSourceRepo();
            _console = console;
            _logger = logger;
        }

        public bool TryGetSettings(out EnvironmentSettings environmentSettings)
        {
            environmentSettings = null;

            var settings = ReadSettingsFromFile();

            // Validate and load settings
            var preparedSettings = PrepareEnvironmentSettings(settings);
            if (IsValid(preparedSettings))
            {
                environmentSettings = preparedSettings;
                return true;
            }

            return false;
        }

        internal IDictionary<string, string> ReadSettingsFromFile()
        {
            if (_settings != null)
            {
                return _settings;
            }

            // Environment variable names in Linux are case-sensitive
            _settings = new Dictionary<string, string>(StringComparer.Ordinal);

            if (_sourceRepo.FileExists(Constants.EnvironmentFileName))
            {
                var lines = _sourceRepo.ReadAllLines(Constants.EnvironmentFileName);
                if (lines != null && lines.Length > 0)
                {
                    foreach (var line in lines)
                    {
                        // Ignore comments and blank lines
                        if (line.StartsWith("#") || string.IsNullOrEmpty(line))
                        {
                            continue;
                        }

                        // Ignore invalid values
                        if (NameAndValuePairParser.TryParse(line, out var key, out var value))
                        {
                            _settings[key] = value;
                        }
                        else
                        {
                            _logger.LogDebug($"Ignoring invalid line '{line}' in '{Constants.EnvironmentFileName}' file.");
                        }
                    }
                }
            }
            else
            {
                _logger.LogDebug($"Could not find file '{Constants.EnvironmentFileName}' to load environment settings.");
            }
            return _settings;
        }

        internal EnvironmentSettings PrepareEnvironmentSettings(IDictionary<string, string> settings)
        {
            var environmentSettings = new EnvironmentSettings
            {
                PreBuildScriptPath = GetPath(EnvironmentSettingsKeys.PreBuildScriptPath),
                PostBuildScriptPath = GetPath(EnvironmentSettingsKeys.PostBuildScriptPath)
            };

            if (!string.IsNullOrEmpty(environmentSettings.PreBuildScriptPath))
            {
                environmentSettings.PreBuildScriptPath = GetScriptAbsolutePath(environmentSettings.PreBuildScriptPath);
            }

            if (!string.IsNullOrEmpty(environmentSettings.PostBuildScriptPath))
            {
                environmentSettings.PostBuildScriptPath = GetScriptAbsolutePath(environmentSettings.PostBuildScriptPath);
            }

            return environmentSettings;

            string GetPath(string name)
            {
                var path = GetValue(name);
                if (path != null)
                {
                    path = path.Trim();
                    var quote = '"';
                    if (path.StartsWith(quote) && path.EndsWith(quote))
                    {
                        return path.Trim(quote);
                    }
                }
                return path;
            }

            string GetValue(string name)
            {
                var prefixedName = Constants.OryxEnvironmentSettingNamePrefix + name;
                if (settings.ContainsKey(prefixedName))
                {
                    return settings[prefixedName];
                }

                if (settings.ContainsKey(name))
                {
                    return settings[name];
                }
                return null;
            }

            string GetScriptAbsolutePath(string path)
            {
                if (!Path.IsPathFullyQualified(path))
                {
                    path = Path.Combine(_sourceRepo.RootPath, path);
                }
                return Path.GetFullPath(path);
            }
        }

        internal bool IsValid(EnvironmentSettings settings)
        {
            if (!string.IsNullOrEmpty(settings.PreBuildScriptPath) &&
                !File.Exists(settings.PreBuildScriptPath))
            {
                _console.WriteLine($"Pre-build script file '{settings.PreBuildScriptPath}' does not exist.");
                return false;
            }

            if (!string.IsNullOrEmpty(settings.PostBuildScriptPath) &&
                !File.Exists(settings.PostBuildScriptPath))
            {
                _console.WriteLine($"Post-build script file '{settings.PostBuildScriptPath}' does not exist.");
                return false;
            }
            return true;
        }
    }
}
