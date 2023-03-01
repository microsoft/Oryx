// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System.Collections.Generic;

namespace Microsoft.Oryx.Automation.Services
{
    public interface IVersionService
    {
        public bool IsVersionWithinRange(string version, string minVersion = null, string maxVersion = null, List<string> blockedVersions = null);
    }
}
