using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Commands
{
    public class CommandBaseProperty
    {
        public string LogFilePath { get; set; }

        public bool DebugMode { get; set; }
    }
}
