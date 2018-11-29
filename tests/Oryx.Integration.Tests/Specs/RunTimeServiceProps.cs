using System;
using System.Collections.Generic;
using System.Text;
using k8s;
using k8s.Models;

namespace Oryx.Integration.Tests.Specs
{
    partial class RunTimeService
    {
        private string Name;

        public static V1Service GetSpec(string name)
        {
            var obj = new RunTimeService
            {
                Name = name
            };
            return Yaml.LoadFromString<V1Service>(obj.TransformText());
        }
    }
}
