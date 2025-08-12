// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.Tests.Common
{
    public class TestExternalSdkProvider : IExternalSdkProvider
    {
        public const string ExternalSdksStorageDir = "/var/OryxSdksCache";

        public Task<XDocument> GetPlatformMetaDataAsync(string platformName)
        {
            return Task.FromResult(null as XDocument);
        }

        public Task<string> GetChecksumForVersionAsync(string platformName, string version)
        {
            return Task.FromResult(string.Empty);
        }

        public Task<bool> RequestBlobAsync(string platformName, string blobName)
        {
            return Task.FromResult(true);
        }
        
    }
}
