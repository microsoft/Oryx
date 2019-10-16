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
        protected const string _imageBaseEnvironmentVariable = "ORYX_TEST_IMAGE_BASE";
        protected const string _defaultImageBase = "oryxdevmcr.azurecr.io";
        protected const string _oryxImageSuffix = "/public/oryx";

        protected readonly ITestOutputHelper _output;
        protected readonly DockerCli _dockerCli = new DockerCli();
        protected readonly string _imageBase;

        public TestBase(ITestOutputHelper output)
        {
            _output = output;
            _imageBase = Environment.GetEnvironmentVariable(_imageBaseEnvironmentVariable);
            if (string.IsNullOrEmpty(_imageBase))
            {
                _output.WriteLine($"Could not find a value for environment variable " +
                                  $"'{_imageBaseEnvironmentVariable}', using default image base '{_defaultImageBase}'.");
                _imageBase = _defaultImageBase;
            }

            _imageBase += _oryxImageSuffix;
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
