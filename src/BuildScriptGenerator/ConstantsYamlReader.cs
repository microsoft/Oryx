// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public static class ConstantsYamlReader
    {
        private static Dictionary<string, string> cache;

        public static string Get(string key) =>
            GetAll().TryGetValue(key, out var v) ? v
            : throw new KeyNotFoundException($"Key '{key}' not found in constants.yml.");

        public static string TryGet(string key) =>
            GetAll().TryGetValue(key, out var v) ? v : null;

        private static Dictionary<string, string> GetAll()
        {
            if (cache != null)
            {
                return cache;
            }

            var path = new[] { Environment.GetEnvironmentVariable("ORYX_CONSTANTS_YAML_PATH"), "/opt/tmp/images/constants.yml" }
                .FirstOrDefault(p => !string.IsNullOrEmpty(p) && File.Exists(p))
                ?? FindInRepo();

            var root = new DeserializerBuilder()
                .WithNamingConvention(NullNamingConvention.Instance)
                .Build()
                .Deserialize<Dictionary<string, Dictionary<string, object>>>(File.ReadAllText(path));

            cache = root?["variables"]?
                .ToDictionary(k => k.Key, k => k.Value?.ToString() ?? string.Empty, StringComparer.OrdinalIgnoreCase);

            return cache;
        }

        private static string FindInRepo()
        {
            var dir = AppContext.BaseDirectory;
            while (dir != null)
            {
                var candidate = Path.Combine(dir, "images", "constants.yml");
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                dir = Directory.GetParent(dir)?.FullName;
            }

            throw new FileNotFoundException("Could not find images/constants.yml.");
        }
    }
}
