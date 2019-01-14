// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal class DefaultEnvironmentSettingsProvider : IEnvironmentSettingsProvider
    {
        private readonly ISourceRepo _sourceRepo;
        private readonly IEnvironment _environment;
        private readonly ILogger<DefaultEnvironmentSettingsProvider> _logger;

        /// <summary>
        /// This service is registered as a singleton, so use the following flag to figure out if settings have already
        /// been loaded to avoid reloading them again.
        /// </summary>
        private bool _loadedSettings;
        private EnvironmentSettings _environmentSettings;

        public DefaultEnvironmentSettingsProvider(
            ISourceRepoProvider sourceRepoProvider,
            IEnvironment environment,
            ILogger<DefaultEnvironmentSettingsProvider> logger)
        {
            _sourceRepo = sourceRepoProvider.GetSourceRepo();
            _environment = environment;
            _logger = logger;
        }

        public bool TryGetAndLoadSettings(out EnvironmentSettings environmentSettings)
        {
            // Avoid loading settings if they have been loaded already
            if (_loadedSettings)
            {
                environmentSettings = _environmentSettings;
                return true;
            }

            environmentSettings = null;

            // Environment variable names in Linux are case-sensitive
            var settings = new Dictionary<string, string>(StringComparer.Ordinal);
            GetSettings(settings);

            // Set them as environment variables
            foreach (var setting in settings)
            {
                _environment.SetEnvironmentVariable(setting.Key, setting.Value);
            }

            _loadedSettings = true;

            // Validate settings
            var preparedSettings = PrepareEnvironmentSettings();
            if (IsValid(preparedSettings))
            {
                // Store the prepared settings so that in later calls this value could be returned quickly.
                _environmentSettings = preparedSettings;

                environmentSettings = _environmentSettings;
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
                _logger.LogDebug(
                    $"Could not find file '{Constants.BuildEnvironmentFileName}' to load environment settings.");
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
                    _logger.LogDebug(
                        $"Ignoring invalid line '{line}' in '{Constants.BuildEnvironmentFileName}' file.");
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
                throw new InvalidUsageException(
                    $"Pre-build script file '{settings.PreBuildScriptPath}' does not exist.");
            }

            if (!string.IsNullOrEmpty(settings.PostBuildScriptPath) &&
                !File.Exists(settings.PostBuildScriptPath))
            {
                throw new InvalidUsageException(
                    $"Post-build script file '{settings.PostBuildScriptPath}' does not exist.");
            }

            return true;
        }
    }
}