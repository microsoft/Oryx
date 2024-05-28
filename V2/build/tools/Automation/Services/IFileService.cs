// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.Automation.Services
{
    public interface IFileService
    {
        void UpdateVersionsToBuildTxt(string platformName, string line);
    }
}