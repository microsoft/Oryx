// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal class DefaultEnvironmentSettingsProvider : IEnvironmentSettingsProvider
    {
        private readonly ISourceRepo _sourceRepo;
        private readonly IEnvironment _environment;
        private readonly IConsole _console;
        private readonly ILogger<DefaultEnvironmentSettingsProvider> _logger;

        public DefaultEnvironmentSettingsProvider(
            ISourceRepoProvider sourceRepoProvider,
            IEnvironment environment,
            IConsole console,
            ILogger<DefaultEnvironmentSettingsProvider> logger)
        {
            _sourceRepo = sourceRepoProvider.GetSourceRepo();
            _environment = environment;
            _console = console;
            _logger = logger;
        }

        public bool TryGetAndLoadSettings(out EnvironmentSettings environmentSettings)
        {
            environmentSettings = null;

            // Environment variable names in Linux are case-sensitive
            var settings = new Dictionary<string, string>(StringComparer.Ordinal);
            GetSettings(settings);

            // Set them as environment variables
            foreach (var setting in settings)
            {
                _environment.SetEnvironmentVariable(setting.Key, setting.Value);
            }

            // Validate settings
            var preparedSettings = PrepareEnvironmentSettings();
            if (IsValid(preparedSettings))
            {
                environmentSettings = preparedSettings;
                return true;
            }

            return false;
        }

        // To enable unit testing
        internal void GetSettings(IDictionary<string, string> settings)
        {
            ReadSettingsFromFile(settings);
            MergeSettingsFromEnvironmentVariables(settings);
        }

        internal void ReadSettingsFromFile(IDictionary<string, string> settings)
        {
            if (!_sourceRepo.FileExists(Constants.BuildEnvironmentFileName))
            {
                _logger.LogDebug($"Could not find file '{Constants.BuildEnvironmentFileName}' to load environment settings.");
                return;
            }

            var lines = _sourceRepo.ReadAllLines(Constants.BuildEnvironmentFileName);
            if (lines == null || lines.Length == 0)
            {
                return;
            }

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
                    settings[key] = value;
                }
                else
                {
                    _logger.LogDebug($"Ignoring invalid line '{line}' in '{Constants.BuildEnvironmentFileName}' file.");
                }
            }
        }

        internal void MergeSettingsFromEnvironmentVariables(IDictionary<string, string> settings)
        {
            if (settings.Count == 0)
            {
                return;
            }

            var currentProcessEnvVariables = _environment.GetEnvironmentVariables();

            var keys = settings.Keys.ToArray();
            foreach (var key in keys)
            {
                if (currentProcessEnvVariables.Contains(key))
                {
                    settings[key] = currentProcessEnvVariables[key] as string;
                }
            }
        }

        internal EnvironmentSettings PrepareEnvironmentSettings()
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
                if (string.IsNullOrEmpty(path))
                {
                    return null;
                }

                path = path.Trim();
                var quote = '"';
                if (path.StartsWith(quote) && path.EndsWith(quote))
                {
                    return path.Trim(quote);
                }

                return path;
            }

            string GetValue(string name)
            {
                var prefixedName = Constants.OryxEnvironmentSettingNamePrefix + name;

                var environmentVariables = _environment.GetEnvironmentVariables();
                if (environmentVariables.Contains(prefixedName))
                {
                    return environmentVariables[prefixedName] as string;
                }

                if (environmentVariables.Contains(name))
                {
                    return environmentVariables[name] as string;
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