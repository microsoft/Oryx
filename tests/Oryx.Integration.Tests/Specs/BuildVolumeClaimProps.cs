using System;
using System.Collections.Generic;
using System.Text;
using k8s;
using k8s.Models;

namespace Microsoft.Oryx.Integration.Tests.Specs
{
    partial class BuildVolumeClaim
    {
        private string Capacity;

        public static V1PersistentVolumeClaim GetSpec(string capacity)
        {
            var obj = new BuildVolumeClaim
            {
                Capacity = capacity
            };
            return Yaml.LoadFromString<V1PersistentVolumeClaim>(obj.TransformText());
        }
    }
}
