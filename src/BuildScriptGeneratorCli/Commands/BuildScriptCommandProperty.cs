using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Commands
{
    public class BuildScriptCommandProperty : BuildCommandBaseProperty
    {
        public string OutputPath { get; set; }
    }
}
