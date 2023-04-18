// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// BuildConfigurationFile class to be used for YAML deserialization.
    /// </summary>
    public class BuildConfigurationFile
    {
        [YamlMember(Alias = "version", ApplyNamingConventions = false)]
        public string Version { get; set; }

        [YamlMember(Alias = "pre-build", ApplyNamingConventions = false)]
        public string Prebuild { get; set; }

        [YamlMember(Alias = "build", ApplyNamingConventions = false)]
        public string Build { get; set; }

        [YamlMember(Alias = "post-build", ApplyNamingConventions = false)]
        public string Postbuild { get; set; }

        [YamlMember(Alias = "run", ApplyNamingConventions = false)]
        public string Run { get; set; }

        public static BuildConfigurationFile Create(string text)
        {
            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();
            var buildConfigFile = deserializer.Deserialize<BuildConfigurationFile>(text);
            return buildConfigFile;
        }
    }
}
