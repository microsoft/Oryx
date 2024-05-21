// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System.Collections.Generic;

namespace Microsoft.Oryx.Automation.Models
{
    /// <Summary>
    /// This is used to deserialize Constants.ConstantsYaml file
    /// </Summary>
    public class ConstantsYamlFile
    {
        public string Name { get; set; } = string.Empty;

        public Dictionary<string, object> Constants { get; set; } = new Dictionary<string, object>();

        public List<object> Outputs { get; set; } = new List<object>();
    }
}