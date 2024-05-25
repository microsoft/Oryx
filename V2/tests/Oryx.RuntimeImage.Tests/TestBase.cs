// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Tests.Common;
using System;
using Xunit.Abstractions;

namespace Microsoft.Oryx.RuntimeImage.Tests
{
    public abstract class TestBase
    {
        protected readonly ITestOutputHelper _output;
        protected readonly DockerCli _dockerCli = new DockerCli();
        protected readonly ImageTestHelper _imageHelper;

        public TestBase(ITestOutputHelper output)
        {
            _output = output;
            _imageHelper = new ImageTestHelper(output);
        }

        protected void RunAsserts(Action action, string message)
        {
            try
            {
                action();
            }
            catch (Exception)
            {
                _output.WriteLine(message);
                throw;
            }
        }
    }
}
