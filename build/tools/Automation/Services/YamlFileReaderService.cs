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
    public class YamlFileReaderService : IYamlFileReaderService
    {
        public async Task<List<ConstantsYamlFile>> ReadConstantsYamlFileAsync(string filePath)
        {
            try
            {
                string fileContents = await File.ReadAllTextAsync(filePath);
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
    }
}
