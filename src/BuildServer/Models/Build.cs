// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildServer.Models
{
    public class Build
    {
        public string Id { get; set; }

        public string Status { get; set; }

        public string Platform { get; set; }

        public string Version { get; set; }

        public string SourcePath { get; set; }

        public string OutputPath { get; set; }

        public string LogPath { get; set; }
    }
}
