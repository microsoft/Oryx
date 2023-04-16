// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

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
            get => this.Platform;
            set
            {
                // Transition to System.Commandline caused this one to set every time when binder is used.
                // Adding this check to set it only when it is actually set by the user.
                this.LanguageWasSet = ((value == null) || (value == string.Empty)) ? false : true;
                this.Platform = value;
            }
        }

        public string LanguageVersion
        {
            get => this.PlatformVersion;
            set
            {
                // Transition to System.Commandline caused this one to set every time when binder is used.
                // Adding this check to set it only when it is actually set by the user.
                this.LanguageVersionWasSet = ((value == null) || (value == string.Empty)) ? false : true;
                this.PlatformVersion = value;
            }
        }

        public string DestinationDir { get; set; }

        public string ManifestDir { get; set; }
    }
}
