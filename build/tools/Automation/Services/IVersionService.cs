// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.Automation.Services
{
    public interface IVersionService
    {
        public bool IsVersionWithinRange(string version, string minVersion = null, string maxVersion = null, string exceptionVersions = null);
    }
}
