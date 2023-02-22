// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Oryx.Automation.Models;

namespace Microsoft.Oryx.Automation.Services
{
    public interface IYamlFileService
    {
        Task<List<ConstantsYamlFile>> ReadConstantsYamlFileAsync(string filePath);

        public void WriteConstantsYamlFile(string filePath, List<ConstantsYamlFile> yamlConstants);
    }
}