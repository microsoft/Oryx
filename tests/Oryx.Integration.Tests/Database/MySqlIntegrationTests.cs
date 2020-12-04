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
    [Trait("category", "db")]
    [Trait("db", "mysql")]
    public class MySqlIntegrationTests : DatabaseTestsBase, IClassFixture<Fixtures.MySqlDbContainerFixture>
    {
        public MySqlIntegrationTests(ITestOutputHelper output, Fixtures.MySqlDbContainerFixture dbFixture)
            : base(output, dbFixture)
        {
        }

        [Theory]
        [InlineData("latest")]
        [InlineData("github-actions")]
        [InlineData("github-actions-buster")]
        public async Task NodeApp_MySqlDB(string imageTag)
        {
            await RunTestAsync(
                "nodejs",
                "10",
                Path.Combine(HostSamplesDir, "nodejs", "node-mysql"),
                buildImageName: _imageHelper.GetBuildImage(imageTag));
        }

        [Theory]
        [InlineData("mysql-pymysql-sample", "latest")]
        [InlineData("mysql-pymysql-sample", "github-actions")]
        [InlineData("mysql-mysqlconnector-sample", "latest")]
        [InlineData("mysql-mysqlconnector-sample", "github-actions")]
        [InlineData("mysql-mysqlclient-sample", "latest")]
        [InlineData("mysql-mysqlclient-sample", "github-actions")]
        public async Task Python37App_MySqlDB_UsingPyMySql_UsingLtsVersionsBuildImage(
            string sampleAppName,
            string imageTag)
        {
            await RunTestAsync(
                "python",
                "3.7",
                Path.Combine(HostSamplesDir, "python", sampleAppName),
                buildImageName: _imageHelper.GetBuildImage(imageTag));
        }
        
        [Theory]
        [InlineData("mysql-pymysql-sample", "github-actions-buster")]
        [InlineData("mysql-mysqlconnector-sample", "github-actions-buster")]
        [InlineData("mysql-mysqlclient-sample", "github-actions-buster")]
        public async Task Python39App_MySqlDB_UsingPyMySql_UsingBusterBuildImage(
            string sampleAppName,
            string imageTag)
        {
            await RunTestAsync(
                "python",
                "3.9",
                Path.Combine(HostSamplesDir, "python", sampleAppName),
                buildImageName: _imageHelper.GetBuildImage(imageTag));
        }

        [Theory]
        [InlineData("7.3", "latest")]
        [InlineData("7.3", "github-actions")]
        [InlineData("7.3", "github-actions-buster")]
        [InlineData("7.2", "latest")]
        [InlineData("7.2", "github-actions")]
        [InlineData("7.0", "latest")]
        [InlineData("7.0", "github-actions")]
        [InlineData("5.6", "latest")]
        [InlineData("5.6", "github-actions")]
        public async Task PhpApp_UsingMysqli(string phpVersion, string imageTag)
        {
            await RunTestAsync(
                "php",
                phpVersion,
                Path.Combine(HostSamplesDir, "php", "mysqli-example"),
                8080,
                specifyBindPortFlag: false,
                buildImageName: _imageHelper.GetBuildImage(imageTag));
        }
    }
}