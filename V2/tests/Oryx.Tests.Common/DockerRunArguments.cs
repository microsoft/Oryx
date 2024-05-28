// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.Tests.Common
{
    public class DockerRunArguments
    {
        public string ImageId { get; set; }
        public List<EnvironmentVariable> EnvironmentVariables { get; set; }
        public IEnumerable<DockerVolume> Volumes { get; set; }
        public int? PortInContainer { get; set; }
        public string Link { get; set; }
        public bool RunContainerInBackground { get; set; }
        public string CommandToExecuteOnRun { get; set; }
        public string[] CommandArguments { get; set; }
        public string WorkingDirectory { get; set; }
    }
}
