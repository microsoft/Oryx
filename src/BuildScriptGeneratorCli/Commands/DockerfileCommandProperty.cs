using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Commands
{
    public class DockerfileCommandProperty : CommandBaseProperty
    {
        public string SourceDir { get; set; }

        public string BuildImage { get; set; }

        public string PlatformName { get; set; }

        public string PlatformVersion { get; set; }

        public string RuntimePlatformName { get; set; }

        public string RuntimePlatformVersion { get; set; }

        public string BindPort { get; set; }

        public string OutputPath { get; set; }
    }
}
