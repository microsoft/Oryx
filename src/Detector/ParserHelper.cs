// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using Nett;
using Newtonsoft.Json.Linq;
using YamlDotNet.RepresentationModel;

namespace Microsoft.Oryx.Detector
{
    /// <summary>
    /// Helper class to parse various files into a native type.
    /// </summary>
    internal static class ParserHelper
    {
        /// <summary>
        /// Parse a .toml file into a TomlTable from the Nett library.
        /// See https://github.com/paiden/Nett for more information.
        /// </summary>
        /// <param name="sourceRepo">Source repo for the application.</param>
        /// <param name="filePath">The path to the .toml file.</param>
        /// <returns>A TomlTable object containing information about the .toml file.</returns>
        public static TomlTable ParseTomlFile(ISourceRepo sourceRepo, string filePath)
        {
            var tomlContent = sourceRepo.ReadFile(filePath);
            return Toml.ReadString(tomlContent);
        }

        /// <summary>
        /// Parse a .yaml file into a YamlMappingNode from the YamlDotNet library.
        /// See https://github.com/aaubry/YamlDotNet for more information.
        /// </summary>
        /// <param name="sourceRepo">Source repo for the application.</param>
        /// <param name="filePath">The path to the .yaml file.</param>
        /// <returns>A YamlMappingNode object containing information about the .yaml file.</returns>
        public static YamlMappingNode ParseYamlFile(ISourceRepo sourceRepo, string filePath)
        {
            var yamlContent = sourceRepo.ReadFile(filePath);
            var yamlStream = new YamlStream();
            yamlStream.Load(new StringReader(yamlContent));
            return (YamlMappingNode)yamlStream.Documents[0].RootNode;
        }

        /// <summary>
        /// Parse a .json file into a JObject from the Newtonsoft.Json library.
        /// </summary>
        /// <param name="sourceRepo">Source repo for the application.</param>
        /// <param name="filePath">The path to the .json file.</param>
        /// <returns>A JObject object containing information about the .json file.</returns>
        public static JObject ParseJsonFile(ISourceRepo sourceRepo, string filePath)
        {
            var jsonContent = sourceRepo.ReadFile(filePath);
            return JObject.Parse(jsonContent);
        }
    }
}
