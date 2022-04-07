using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// BuildConfigurationFIle class to be used for YAML deserialization.
    /// </summary>
    public class BuildConfigurationFIle
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

        public static BuildConfigurationFIle Create(string text)
        {
            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();
            var buildConfigFile = deserializer.Deserialize<BuildConfigurationFIle>(text);
            return buildConfigFile;
        }
    }
}
