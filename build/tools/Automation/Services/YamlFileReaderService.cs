// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Oryx.Automation.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Microsoft.Oryx.Automation.Services
{
    public class YamlFileReaderService : IYamlFileReaderService
    {
        public async Task<List<ConstantsYamlFile>> ReadConstantsYamlFileAsync(string filePath)
        {
            string fileContents = await File.ReadAllTextAsync(filePath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();
            var yamlContents = deserializer.Deserialize<List<ConstantsYamlFile>>(fileContents);

            return yamlContents;
        }
    }
}
