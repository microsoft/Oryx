using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Commands
{
    public class BuildCommandProperty : BuildCommandBaseProperty
    {
        public bool LanguageVersionWasSet { get; set; }

        public bool LanguageWasSet { get; set; }

        public string IntermediateDir { get; set; }

        public string LanguageName
        {
            get => this.PlatformName;
            set
            {
                this.PlatformName = value;
                this.LanguageWasSet = true;
            }
        }

        public string LanguageVersion
        {
            get => this.PlatformVersion;
            set
            {
                this.PlatformVersion = value;
                this.LanguageVersionWasSet = true;
            }
        }

        public string DestinationDir { get; set; }

        public string ManifestDir { get; set; }
    }
}
