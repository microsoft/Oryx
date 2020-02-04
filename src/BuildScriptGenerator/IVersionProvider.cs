// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public interface IVersionProvider
    {
        IEnumerable<VersionInfo> GetSupportedVersions(string platformName);
    }

    public class VersionInfo
    {
        public VersionSource VersionSource { get; set; }

        public string Version { get; set; }
    }

    public enum VersionSource
    {
        Disk,
        AzureStorage
    }

    public class FromDiskVersionProvider : IVersionProvider
    {
        public IEnumerable<VersionInfo> GetSupportedVersions(string platformName)
        {
           return VersionProviderHelper.GetVersionsFromDirectory()
        }
    }
}
