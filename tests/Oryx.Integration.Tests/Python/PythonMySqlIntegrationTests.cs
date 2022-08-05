// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

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
        [InlineData("mysql-pymysql-sample", "latest-stretch")]
        [InlineData("mysql-pymysql-sample", "github-actions-stretch")]
        [InlineData("mysql-mysqlconnector-sample", "latest-stretch")]
        [InlineData("mysql-mysqlconnector-sample", "github-actions-stretch")]
        [InlineData("mysql-mysqlclient-sample", "latest-stretch")]
        [InlineData("mysql-mysqlclient-sample", "github-actions")]
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
        [InlineData("mysql-pymysql-sample", "github-actions-buster")]
        [InlineData("mysql-mysqlconnector-sample", "github-actions-buster")]
        [InlineData("mysql-mysqlclient-sample", "github-actions-buster")]
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