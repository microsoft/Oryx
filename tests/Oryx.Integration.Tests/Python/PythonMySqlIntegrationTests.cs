// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Tests.Common;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("db", "mysql")]
    public class PythonMySqlIntegrationTests : DatabaseTestsBase, IClassFixture<Fixtures.MySqlDbContainerFixture>
    {
        public PythonMySqlIntegrationTests(ITestOutputHelper output, Fixtures.MySqlDbContainerFixture dbFixture)
            : base(output, dbFixture)
        {
        }

        [Theory(Skip = "Bug #1410367")]
        [Trait("category", "python-3.7")]
        [Trait("build-image", "debian-stretch")]
        [InlineData("mysql-pymysql-sample")]
        [InlineData("mysql-mysqlconnector-sample")]
        [InlineData("mysql-mysqlclient-sample")]
        public async Task Python37App_MySqlDB_UsingPyMySql_UsingLatestStretchBuildImageAsync(string sampleAppName)
        {
            await PythonApp_MySqlDB_UsingPyMySqlAsync("3.7", ImageTestHelperConstants.OsTypeDebianBullseye, sampleAppName, ImageTestHelperConstants.LatestStretchTag);
        }

        [Theory(Skip = "Bug #1410367")]
        [Trait("category", "python-3.7")]
        [Trait("build-image", "github-actions-debian-bullseye")]
        [InlineData("mysql-pymysql-sample")]
        [InlineData("mysql-mysqlconnector-sample")]
        [InlineData("mysql-mysqlclient-sample")]
        public async Task Python37App_MySqlDB_UsingPyMySql_UsingGitHubActionsBullseyeBuildImageAsync(string sampleAppName)
        {
            await PythonApp_MySqlDB_UsingPyMySqlAsync("3.7", ImageTestHelperConstants.OsTypeDebianBullseye, sampleAppName, ImageTestHelperConstants.GitHubActionsBullseye);
        }

        [Theory(Skip = "Bug #1410367")]
        [Trait("category", "githubactions")]
        [Trait("build-image", "github-actions-debian-buster")]
        [InlineData("mysql-pymysql-sample")]
        [InlineData("mysql-mysqlconnector-sample")]
        [InlineData("mysql-mysqlclient-sample")]
        public async Task Python39App_MySqlDB_UsingPyMySql_UsingGitHubActionsBusterBuildImageAsync(string sampleAppName)
        {
            await PythonApp_MySqlDB_UsingPyMySqlAsync("3.9", ImageTestHelperConstants.OsTypeDebianBullseye, sampleAppName, ImageTestHelperConstants.GitHubActionsBuster);
        }

        private async Task PythonApp_MySqlDB_UsingPyMySqlAsync(
            string pythonVersion,
            string osType,
            string sampleAppName,
            string imageTag)
        {
            await RunTestAsync(
                "python",
                pythonVersion,
                osType,
                Path.Combine(HostSamplesDir, "python", sampleAppName),
                buildImageName: _imageHelper.GetBuildImage(imageTag));
        }
    }
}