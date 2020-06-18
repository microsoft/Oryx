// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using Microsoft.Oryx.BuildScriptGenerator.Hugo;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class StaticSiteGeneratorHelperTest
    {
        protected readonly string _nodeSampleDir = Path.Combine(Directory.GetCurrentDirectory(), "SampleApps", "hugo");

        [Theory]
        [InlineData("hugo-sample")]
        [InlineData("hugo-sample-json")]
        [InlineData("hugo-sample-yaml")]
        [InlineData("hugo-sample-yml")]
        public void Validate_IsHugoApp(string appName)
        {
            var environment = new TestEnvironment();
            var sourceDirectory = Path.Combine(_nodeSampleDir, appName);
            var sourceRepo = new LocalSourceRepo(sourceDirectory);
            var result = StaticSiteGeneratorHelper.IsHugoApp(sourceRepo, environment);

            Assert.True(result);
        }
    }
}
