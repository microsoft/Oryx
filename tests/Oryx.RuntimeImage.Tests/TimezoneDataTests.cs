// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.RuntimeImage.Tests
{
    public class TimezoneDataTests : TestBase
    {
        public TimezoneDataTests(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [Trait("category", "runtime-bullseye")]
        [InlineData("php", "8.4-fpm")]
        [InlineData("dotnetcore", "8.0")]
        public void BullseyeRuntimeImage_ContainsUpdatedTimezoneData(string platform, string version)
        {
            VerifyTimezoneData(
                platform,
                version,
                ImageTestHelperConstants.OsTypeDebianBullseye,
                verifyDebianCompatibility: true);
        }

        [Theory]
        [Trait("category", "runtime-bookworm")]
        [InlineData("php", "8.4-fpm")]
        [InlineData("dotnetcore", "8.0")]
        [InlineData("dotnetcore", "9.0")]
        public void BookwormRuntimeImage_ContainsUpdatedTimezoneData(string platform, string version)
        {
            VerifyTimezoneData(
                platform,
                version,
                ImageTestHelperConstants.OsTypeDebianBookworm,
                verifyDebianCompatibility: true);
        }

        [Theory]
        [Trait("category", "runtime-noble")]
        [InlineData("php", "8.5-fpm")]
        [InlineData("dotnetcore", "10.0")]
        public void NobleRuntimeImage_ContainsUpdatedTimezoneData(string platform, string version)
        {
            VerifyTimezoneData(
                platform,
                version,
                ImageTestHelperConstants.OsTypeUbuntuNoble,
                verifyDebianCompatibility: false);
        }

        private void VerifyTimezoneData(
            string platform,
            string version,
            string osType,
            bool verifyDebianCompatibility)
        {
            var assertions =
                "dpkg-query -W tzdata >/dev/null" +
                " && test \"$(TZ=America/Winnipeg date -d '2026-12-01 12:00:00' +%z)\" = '-0500'" +
                " && test \"$(TZ=America/Edmonton date -d '2026-12-01 12:00:00' +%z)\" = '-0600'";

            if (verifyDebianCompatibility)
            {
                assertions +=
                    " && test -f /etc/timezone" +
                    " && test -L /usr/share/zoneinfo/posix" +
                    " && test \"$(readlink /usr/share/zoneinfo/posix)\" = '.'" +
                    " && test \"$(TZ=posix/America/Winnipeg date -d '2026-12-01 12:00:00' +%z)\" = '-0500'";
            }

            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage(platform, version, osType),
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", assertions },
            });

            RunAsserts(
                () => Assert.True(result.IsSuccess),
                result.GetDebugInfo());
        }
    }
}
