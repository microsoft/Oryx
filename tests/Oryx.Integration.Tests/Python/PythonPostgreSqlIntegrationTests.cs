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
    [Trait("db", "postgres")]
    public class PythonPostgreSqlIntegrationTests : DatabaseTestsBase, IClassFixture<Fixtures.PostgreSqlDbContainerFixture>
    {
        public PythonPostgreSqlIntegrationTests(ITestOutputHelper output, Fixtures.PostgreSqlDbContainerFixture dbFixture)
            : base(output, dbFixture)
        {
        }

        [Theory(Skip = "Bug 1410367") ]
        [InlineData(ImageTestHelperConstants.GitHubActionsStretch)]
        [InlineData("github-actions-buster")]
        [InlineData("latest-stretch")]
        public async Task Python37App_PostgreSqlDBAsync(string imageTag)
        {
            await RunTestAsync(
                "python",
                "3.7",
                Path.Combine(HostSamplesDir, "python", "postgres-sample"),
                buildImageName: _imageHelper.GetBuildImage(imageTag));
        }

    }
}