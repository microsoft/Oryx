// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class CheckerExtensionsTest
    {
        [Fact]
        public void WhereApplicable_Sanity()
        {
            var a = new CheckerA();
            var b = new CheckerB();
            var both = new IChecker[] { a, b };

            Assert.Empty(both.WhereApplicable(new Dictionary<string, string> { { "c", "1" } }));
            Assert.Equal(a,    both.WhereApplicable(new Dictionary<string, string> { { "a", "1" } }).Single());
            Assert.Equal(b,    both.WhereApplicable(new Dictionary<string, string> { { "b", "1" } }).Single());
            Assert.Equal(both, both.WhereApplicable(new Dictionary<string, string> { { "a", "1" }, { "b", "1" } }));
        }
    }

    [Checker("a")]
    class CheckerA : IChecker
    {
        public IEnumerable<ICheckerMessage> CheckSourceRepo(ISourceRepo repo) =>
            Enumerable.Empty<ICheckerMessage>();

        public IEnumerable<ICheckerMessage> CheckToolVersions(IDictionary<string, string> toolsToVersions) =>
            Enumerable.Empty<ICheckerMessage>();
    }

    [Checker("b")]
    class CheckerB : IChecker
    {
        public IEnumerable<ICheckerMessage> CheckSourceRepo(ISourceRepo repo) =>
            Enumerable.Empty<ICheckerMessage>();

        public IEnumerable<ICheckerMessage> CheckToolVersions(IDictionary<string, string> toolsToVersions) =>
            Enumerable.Empty<ICheckerMessage>();
    }
}
