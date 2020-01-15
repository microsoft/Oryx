// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using System.Linq;
using Scriban.Syntax;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class StaticSiteGeneratorHelperTest
    {
        protected readonly string _nodeSampleDir = Path.Combine(Directory.GetCurrentDirectory(), "SampleApps", "nodejs");

        [Theory]
        [InlineData("hugo-sample")]
        [InlineData("hugo-sample-json")]
        [InlineData("hugo-sample-yaml")]
        public void Validate_IsHugoApp(string appName)
        {
            var sourceDirectory = Path.Combine(_nodeSampleDir, appName);
            var sourceRepo = new LocalSourceRepo(sourceDirectory);
            var result = StaticSiteGeneratorHelper.IsHugoApp(sourceRepo);

            Assert.True(result);
        }
    }
}
