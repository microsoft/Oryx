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
    [Trait("category", "python")]
    [Trait("db", "mysql")]
    public class PythonMySqlIntegrationTests : DatabaseTestsBase, IClassFixture<Fixtures.MySqlDbContainerFixture>
    {
        public PythonMySqlIntegrationTests(ITestOutputHelper output, Fixtures.MySqlDbContainerFixture dbFixture)
            : base(output, dbFixture)
        {
        }

        [Theory(Skip = "Bug 1410367") ]
        [InlineData("mysql-pymysql-sample", ImageTestHelperConstants.LatestStretchTag)]
        [InlineData("mysql-pymysql-sample", ImageTestHelperConstants.GitHubActionsStretch)]
        [InlineData("mysql-mysqlconnector-sample", ImageTestHelperConstants.LatestStretchTag)]
        [InlineData("mysql-mysqlconnector-sample", ImageTestHelperConstants.GitHubActionsStretch)]
        [InlineData("mysql-mysqlclient-sample", ImageTestHelperConstants.LatestStretchTag)]
        [InlineData("mysql-mysqlclient-sample", ImageTestHelperConstants.GitHubActionsStretch)]
        public async Task Python37App_MySqlDB_UsingPyMySql_UsingLtsVersionsBuildImageAsync(
            string sampleAppName,
            string imageTag)
        {
            await RunTestAsync(
                "python",
                "3.7",
                Path.Combine(HostSamplesDir, "python", sampleAppName),
                buildImageName: _imageHelper.GetBuildImage(imageTag));
        }

        [Theory(Skip = "Bug 1410367") ]
        [InlineData("mysql-pymysql-sample", ImageTestHelperConstants.GitHubActionsBuster)]
        [InlineData("mysql-mysqlconnector-sample", ImageTestHelperConstants.GitHubActionsBuster)]
        [InlineData("mysql-mysqlclient-sample", ImageTestHelperConstants.GitHubActionsBuster)]
        public async Task Python39App_MySqlDB_UsingPyMySql_UsingBusterBuildImageAsync(
            string sampleAppName,
            string imageTag)
        {
            await RunTestAsync(
                "python",
                "3.9",
                Path.Combine(HostSamplesDir, "python", sampleAppName),
                buildImageName: _imageHelper.GetBuildImage(imageTag));
        }
    }
}