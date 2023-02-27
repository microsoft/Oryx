// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Oryx.Automation.Models;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Microsoft.Oryx.Automation.Services
{
    public class YamlFileService : IYamlFileService
    {
        private string oryxRootPath;

        public YamlFileService(string oryxRootPath)
        {
            this.oryxRootPath = oryxRootPath;
        }

        public async Task<List<ConstantsYamlFile>> ReadConstantsYamlFileAsync(string subPath)
        {
            try
            {
                string absolutePath = Path.Combine(this.oryxRootPath, subPath);
                string fileContents = await File.ReadAllTextAsync(absolutePath);
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .Build();
                var yamlContents = deserializer.Deserialize<List<ConstantsYamlFile>>(fileContents);

                return yamlContents;
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Error reading YAML file: {ex.Message}");
                throw new ArgumentException("YAML file not found.", ex);
            }
            catch (YamlException ex)
            {
                Console.WriteLine($"Error reading YAML file: {ex.Message}");
                throw new ArgumentException("Invalid YAML file format.", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading YAML file: {ex.Message}");
                throw;
            }
        }

        public void WriteConstantsYamlFile(string subPath, List<ConstantsYamlFile> yamlConstants)
        {
            try
            {
                var serializer = new SerializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .Build();

                string absolutePath = Path.Combine(this.oryxRootPath, subPath);
                var stringResult = serializer.Serialize(yamlConstants);
                File.WriteAllText(absolutePath, stringResult);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Error writing constants YAML file: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while writing the YAML file: {ex.Message}");
            }
        }
    }
}
