using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Commands
{
    public class BuildpackDetectCommandProperty : CommandBaseProperty
    {
        public string PlanPath { get; set; }

        public string PlatformDir { get; set; }

        public string SourceDir { get; set; }
    }
}
