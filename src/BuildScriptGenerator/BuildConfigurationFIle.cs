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
        public string version { get; set; }

        [YamlMember(Alias = "pre-build", ApplyNamingConventions = false)]
        public string prebuild { get; set; }

        [YamlMember(Alias = "build", ApplyNamingConventions = false)]
        public string build { get; set; }

        [YamlMember(Alias = "post-build", ApplyNamingConventions = false)]
        public string postbuild { get; set; }

        [YamlMember(Alias = "run", ApplyNamingConventions = false)]
        public string run { get; set; }

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
