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
        private static IChecker A = new CheckerA();
        private static IChecker B = new CheckerB();
        private static IChecker G = new CheckerGlobal();
        private static IChecker[] AllCheckers = new IChecker[] { A, B, G };

        [Fact]
        public void WhereApplicable_KeepsGlobalCheckers()
        {
            Assert.Equal(G, AllCheckers.WhereApplicable(new Dictionary<string, string> { { "c", "1" } }).Single());
        }

        [Fact]
        public void WhereApplicable_KeepsOneRequiredChecker()
        {
            Assert.Equal(new IChecker[] { A, G },
                AllCheckers.WhereApplicable(new Dictionary<string, string> { { "a", "1" } }));

            Assert.Equal(new IChecker[] { B, G },
                AllCheckers.WhereApplicable(new Dictionary<string, string> { { "b", "1" } }));
        }

        [Fact]
        public void WhereApplicable_KeepsAllRequiredCheckers()
        {
            Assert.Equal(AllCheckers,
                AllCheckers.WhereApplicable(new Dictionary<string, string> { { "a", "1" }, { "b", "1" } }));
        }
    }

    class CheckerBase : IChecker
    {
        public IEnumerable<ICheckerMessage> CheckSourceRepo(ISourceRepo repo) =>
            Enumerable.Empty<ICheckerMessage>();

        public IEnumerable<ICheckerMessage> CheckToolVersions(IDictionary<string, string> toolsToVersions) =>
            Enumerable.Empty<ICheckerMessage>();
    }

    [Checker("a")]
    class CheckerA : CheckerBase { }

    [Checker("b")]
    class CheckerB : CheckerBase { }

    [Checker]
    class CheckerGlobal : CheckerBase { }
}
