using System;
using System.Collections.Generic;
using System.Text;
using k8s;
using k8s.Models;

namespace Oryx.Integration.Tests.Specs
{
    partial class RunTimeDeployment
    {
        private string Name;
        private string AppName;
        private string Image;
        private string AppDir;
        private string VolumeName;
        private string AzureFileShareName;

        public static V1Deployment GetSpec(string name, string appName, string image, string appDir, string volumeName, string azShareName)
        {
            var obj = new RunTimeDeployment
            {
                Name = name,
                AppName = appName,
                Image = image,
                AppDir = appDir,
                VolumeName = volumeName,
                AzureFileShareName = azShareName,
            };
            return Yaml.LoadFromString<V1Deployment>(obj.TransformText());
        }
    }
}
