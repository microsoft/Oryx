// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System.Collections.Generic;
using NuGet.Versioning;
using Xunit;

namespace Microsoft.Oryx.Automation.Services.Tests
{
    public class VersionServiceTests
    {
        private readonly VersionService _versionService = new VersionService();

        [Theory]
        [InlineData("1.0.0", null, null, null, true)]
        [InlineData("1.2.3", "1.0.0", "2.0.0", null, true)]
        [InlineData("2.0.0", "1.0.0", "2.0.0", null, true)]
        [InlineData("0.9.0", "1.0.0", "2.0.0", null, false)]
        [InlineData("2.1.0", "1.0.0", "2.0.0", null, false)]
        [InlineData("1.0.0-preview.1", null, null, null, false)]
        [InlineData("1.2.3-preview.4", "1.0.0", "2.0.0", null, false)]
        [InlineData("2.0.0-alpha", "1.0.0", "1.9.9", null, false)]
        [InlineData("1.2.3", null, null, new[] { "1.2.3" }, false)]
        [InlineData("1.2.3", null, null, new[] { "1.2.2" }, true)]
        [InlineData("1.2.3", null, null, new[] { "1.2.4" }, true)]
        [InlineData("1.2.3", null, null, new[] { "1.2.2", "1.2.4" }, true)]
        [InlineData("7.0.0-rc.1.23419.4", null, null, null, false)]
        [InlineData("8.0.0-rc.1.23419.4", null, null, null, true)]
        public void IsVersionWithinRange_ReturnsExpectedResult(
            string version, string minVersion, string maxVersion, string[] blockedVersions, bool expectedResult)
        {
            var blockedVersionsList = blockedVersions != null ? new List<string>(blockedVersions) : null;

            var result = _versionService.IsVersionWithinRange(version, minVersion, maxVersion, blockedVersionsList);

            Assert.Equal(expectedResult, result);
        }
    }
}