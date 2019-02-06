using System;
using System.Collections.Generic;
using System.Text;
using k8s;
using k8s.Models;

namespace Microsoft.Oryx.Integration.Tests.Specs
{
    partial class BuildVolume
    {
        private string Name;
        private string Capacity;
        private string AzureFileShareName;

        public static V1PersistentVolume GetSpec(string name, string capacity, string azShareName)
        {
            var obj = new BuildVolume
            {
                Name = name,
                Capacity = capacity,
                AzureFileShareName = azShareName,
            };
            return Yaml.LoadFromString<V1PersistentVolume>(obj.TransformText());
        }
    }
}
