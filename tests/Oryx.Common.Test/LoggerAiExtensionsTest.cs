// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Xunit;

namespace Oryx.Common.Test
{
    public class LoggerAiExtensionsTest
    {
        [Fact]
        public void SplitByLength_Sanity()
        {
        	// Empty string
        	// ...

        	// Chunks of equal lengths
        	LoggerAiExtensions.Chunkify("abcabc", 3);

        	// Chunks of different lengths
        }
    }
}
