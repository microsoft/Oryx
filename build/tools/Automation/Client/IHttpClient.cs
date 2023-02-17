// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Oryx.Automation.Client
{
    public interface IHttpClient
    {
        Task<string> GetDataAsync(string url);

        Task<HashSet<string>> GetOryxSdkVersionsAsync(string url);
    }
}
