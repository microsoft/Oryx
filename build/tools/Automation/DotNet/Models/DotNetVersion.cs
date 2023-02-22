// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.Automation.DotNet.Models
{
    // </Summary>
    public class DotNetVersion
    {
        /// <Summary>
        /// The version of the platfom.
        /// Example: 1.2.3
        /// </Summary>
        public string Version { get; set; } = string.Empty;

        /// <Summary>
        /// The sha of the platform's version.
        /// Some platforms may not have a sha.
        /// </Summary>
        public string Sha { get; set; } = string.Empty;

        /// <Summary>
        /// The type of version that is being represented.
        /// Example: sdk, aspnetcore, netcore, etc.
        /// </Summary>
        public string VersionType { get; set; } = string.Empty;
    }
}
