// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public static class ConstantsYamlReader
    {
        private static readonly object LockObj = new object();
        private static Dictionary<string, string> constants;

        public static string Get(string key)
        {
            EnsureLoaded();
            if (constants.TryGetValue(key, out var value))
            {
                return value;
            }

            throw new KeyNotFoundException(
                $"Key '{key}' not found in constants.yml.");
        }

        public static string TryGet(string key)
        {
            EnsureLoaded();
            return constants.TryGetValue(key, out var value) ? value : null;
        }

        public static void Reload()
        {
            lock (LockObj)
            {
                constants = null;
            }
        }

        public static void Override(string key, string value)
        {
            EnsureLoaded();
            lock (LockObj)
            {
                constants[key] = value;
            }
        }

        private static void EnsureLoaded()
        {
            if (constants != null)
            {
                return;
            }

            lock (LockObj)
            {
                if (constants != null)
                {
                    return;
                }

                var filePath = ResolveFilePath();
                constants = ParseYaml(File.ReadAllText(filePath));
            }
        }

        private static string ResolveFilePath()
        {
            var envPath = Environment.GetEnvironmentVariable("ORYX_CONSTANTS_YAML_PATH");
            if (!string.IsNullOrEmpty(envPath) && File.Exists(envPath))
            {
                return envPath;
            }

            var containerPath = "/opt/tmp/images/constants.yml";
            if (File.Exists(containerPath))
            {
                return containerPath;
            }

            var dir = AppContext.BaseDirectory;
            for (int i = 0; i < 10; i++)
            {
                var candidate = Path.Combine(dir, "images", "constants.yml");
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                var parent = Directory.GetParent(dir);
                if (parent == null)
                {
                    break;
                }

                dir = parent.FullName;
            }

            throw new FileNotFoundException(
                "Could not find images/constants.yml. " +
                "Set ORYX_CONSTANTS_YAML_PATH or ensure the file exists in the repo root.");
        }

        private static Dictionary<string, string> ParseYaml(string content)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            bool inVariables = false;

            foreach (var rawLine in content.Split('\n'))
            {
                var line = rawLine.TrimEnd('\r');

                if (line.TrimStart().StartsWith("variables:"))
                {
                    inVariables = true;
                    continue;
                }

                if (!inVariables)
                {
                    continue;
                }

                if (line.Length > 0 && line[0] != ' ' && line[0] != '#')
                {
                    break;
                }

                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                {
                    continue;
                }

                var colonIndex = trimmed.IndexOf(':');
                if (colonIndex <= 0)
                {
                    continue;
                }

                var key = trimmed.Substring(0, colonIndex).Trim();
                var value = trimmed.Substring(colonIndex + 1).Trim();

                if (value.Length >= 2 &&
                    ((value[0] == '"' && value[value.Length - 1] == '"') ||
                     (value[0] == '\'' && value[value.Length - 1] == '\'')))
                {
                    value = value.Substring(1, value.Length - 2);
                }

                result[key] = value;
            }

            return result;
        }
    }
}
