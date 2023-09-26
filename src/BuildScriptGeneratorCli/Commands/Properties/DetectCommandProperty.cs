// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Commands
{
    public class DetectCommandProperty : CommandBaseProperty
    {
        public string SourceDir { get; set; }

        public string Platform { get; set; }

        public string OutputFormat { get; set; }
    }
}
