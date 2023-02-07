using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Commands
{
    public class ExecCommandProperty : CommandBaseProperty
    {
        public string SourceDir { get; set; }

        public string Command { get; set; }
    }
}
