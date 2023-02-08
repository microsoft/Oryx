using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Commands
{
    public class TelemetryCommandProperty : CommandBaseProperty
    {
        public string EventName { get; set; }

        public double ProcessingTime { get; set; }

        public string[] Properties { get; set; }
    }
}
