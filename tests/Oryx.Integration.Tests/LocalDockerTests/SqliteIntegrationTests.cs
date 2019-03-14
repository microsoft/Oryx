// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests.LocalDockerTests
{
    public class SqliteIntegrationTests : DatabaseTestsBase
    {
        private const int hostPort = 8088;

        public SqliteIntegrationTests(ITestOutputHelper output) : base(output, hostPort)
        {
        }

        [Fact]
        public async Task Python37App_SqlLiteDB()
        {
            await RunTestAsync(
                "python",
                "3.7",
                Path.Combine(HostSamplesDir, "python", "sqllite-sample"));
        }
    }
}