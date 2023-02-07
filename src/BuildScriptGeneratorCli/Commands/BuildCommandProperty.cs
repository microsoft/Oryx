using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Commands
{
    public class BuildCommandProperty : BuildCommandBaseProperty
    {
        private bool languageVersionWasSet;
        private bool languageWasSet;

        public string IntermediateDir { get; set; }

        public string LanguageName
        {
            get => this.PlatformName;
            set
            {
                this.PlatformName = value;
                this.languageWasSet = true;
            }
        }

        public string LanguageVersion
        {
            get => this.PlatformVersion;
            set
            {
                this.PlatformVersion = value;
                this.languageVersionWasSet = true;
            }
        }

        public string DestinationDir { get; set; }

        public string ManifestDir { get; set; }
    }
}
