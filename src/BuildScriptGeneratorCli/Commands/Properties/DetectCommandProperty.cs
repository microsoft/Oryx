using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Commands
{
    public class DetectCommandProperty : CommandBaseProperty
    {
        public string SourceDir { get; set; }

        public string OutputFormat { get; set; }
    }
}
