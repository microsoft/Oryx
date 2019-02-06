using System;
using System.Collections.Generic;
using System.Text;
using k8s;
using k8s.Models;

namespace Microsoft.Oryx.Integration.Tests.Specs
{
    partial class BuildPod
    {
        private string Name;
        private string VolumeName;
        private string VolumeClaimName;

        public static V1Pod GetSpec(string name, string volumeName, string volumeClaimName)
        {
            var obj = new BuildPod
            {
                Name = name,
                VolumeName = volumeName,
                VolumeClaimName = volumeClaimName
            };
            return Yaml.LoadFromString<V1Pod>(obj.TransformText());
        }
    }
}
