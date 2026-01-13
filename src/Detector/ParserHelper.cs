// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Oryx.Detector.Exceptions;
using Microsoft.Oryx.Detector.Resources;
using Newtonsoft.Json.Linq;
using Tomlyn;
using Tomlyn.Model;
using YamlDotNet.RepresentationModel;

namespace Microsoft.Oryx.Detector
{
    /// <summary>
    /// Helper class to parse various files into a native type.
    /// </summary>
    internal static class ParserHelper
    {
        /// <summary>
        /// Maximum allowed file size for configuration files (10 MB).
        /// </summary>
        private const long MaxConfigurationFileSizeInBytes = 10 * 1024 * 1024; // 10 MB

        /// <summary>
        /// Parse a .toml file into a TomlTable from the Tomlyn library.
        /// See https://github.com/xoofx/Tomlyn for more information.
        /// </summary>
        /// <param name="sourceRepo">Source repo for the application.</param>
        /// <param name="filePath">The path to the .toml file.</param>
        /// <returns>A TomlTable object containing information about the .toml file.</returns>
        public static TomlTable ParseTomlFile(ISourceRepo sourceRepo, string filePath)
        {
            ValidateFileSizeBeforeReading(sourceRepo, filePath);
            var tomlContent = sourceRepo.ReadFile(filePath);

            try
            {
                // Gets a syntax tree of the TOML text
                var doc = Toml.Parse(tomlContent);

                // Gets a runtime representation of the syntax tree
                var table = doc.ToModel();
                return table;
            }
            catch (Exception ex)
            {
                throw new FailedToParseFileException(
                    filePath,
                    string.Format(Messages.FailedToParseFileExceptionFormat, filePath),
                    ex);
            }
        }

        /// <summary>
        /// Parse a .yaml file into a YamlMappingNode from the YamlDotNet library.
        /// See https://github.com/aaubry/YamlDotNet for more information.
        /// </summary>
        /// <param name="sourceRepo">Source repo for the application.</param>
        /// <param name="filePath">The path to the .yaml file.</param>
        /// <returns>A YamlMappingNode object containing information about the .yaml file.</returns>
        public static YamlNode ParseYamlFile(ISourceRepo sourceRepo, string filePath)
        {
            ValidateFileSizeBeforeReading(sourceRepo, filePath);
            var yamlContent = sourceRepo.ReadFile(filePath);
            var yamlStream = new YamlStream();

            try
            {
                yamlStream.Load(new StringReader(yamlContent));
                return yamlStream.Documents[0].RootNode;
            }
            catch (Exception ex)
            {
                throw new FailedToParseFileException(
                    filePath,
                    string.Format(Messages.FailedToParseFileExceptionFormat, filePath),
                    ex);
            }
        }

        /// <summary>
        /// Parse a .json file into a JObject from the Newtonsoft.Json library.
        /// </summary>
        /// <param name="sourceRepo">Source repo for the application.</param>
        /// <param name="filePath">The path to the .json file.</param>
        /// <returns>A JObject object containing information about the .json file.</returns>
        public static JObject ParseJsonFile(ISourceRepo sourceRepo, string filePath)
        {
            ValidateFileSizeBeforeReading(sourceRepo, filePath);
            var jsonContent = sourceRepo.ReadFile(filePath);
            return JObject.Parse(jsonContent);
        }

        /// <summary>
        /// Validates that a file size does not exceed the maximum allowed size before reading it into memory.
        /// </summary>
        /// <param name="sourceRepo">Source repo for the application.</param>
        /// <param name="filePath">The path to the file to validate.</param>
        /// <exception cref="InvalidOperationException">Thrown when file size exceeds the maximum allowed size.</exception>
        private static void ValidateFileSizeBeforeReading(ISourceRepo sourceRepo, string filePath)
        {
            var fileSize = sourceRepo.GetFileSize(filePath);
            
            if (fileSize > MaxConfigurationFileSizeInBytes)
            {
                var fileSizeMB = fileSize / (1024.0 * 1024.0);
                var maxSizeMB = MaxConfigurationFileSizeInBytes / (1024.0 * 1024.0);
                throw new InvalidOperationException(
                    $"Configuration file '{filePath}' is too large ({fileSizeMB:F2} MB). " +
                    $"The maximum allowed size is {maxSizeMB:F2} MB.");
            }
        }
    }
}
