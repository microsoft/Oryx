// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Oryx.Automation.Services
{
    public interface IHttpServiceExtension
    {
        public Task<string> GetDataAsync(string url);

        public Task<HashSet<string>> GetOryxSdkVersionsAsync(string url);
    }
}