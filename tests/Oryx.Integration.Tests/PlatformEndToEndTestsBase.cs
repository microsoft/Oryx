// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    public abstract class PlatformEndToEndTestsBase : IClassFixture<TestTempDirTestFixture>
    {
        protected const string _imageBaseEnvironmentVariable = "ORYX_TEST_IMAGE_BASE";
        protected const string _defaultImageBase = "oryxdevmcr.azurecr.io";
        protected const string _oryxImageSuffix = "/public/oryx";

        protected readonly HttpClient _httpClient = new HttpClient();
        protected readonly ITestOutputHelper _output;
        protected readonly string _hostSamplesDir;
        protected readonly string _tempRootDir;
        protected readonly string _imageBase;

        public PlatformEndToEndTestsBase(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
        {
            _output = output;
            _hostSamplesDir = Path.Combine(Directory.GetCurrentDirectory(), "SampleApps");
            _tempRootDir = testTempDirTestFixture.RootDirPath;
            _imageBase = Environment.GetEnvironmentVariable(_imageBaseEnvironmentVariable);
            if (string.IsNullOrEmpty(_imageBase))
            {
                _output.WriteLine($"Could not find a value for environment variable " +
                                  $"'{_imageBaseEnvironmentVariable}', using default image base '{_defaultImageBase}'.");
                _imageBase = _defaultImageBase;
            }

            _imageBase += _oryxImageSuffix;
        }

        // The following method is used to avoid following exception from HttpClient when trying to read a response:
        // '"utf-8"' is not a supported encoding name. For information on defining a custom encoding,
        // see the documentation for the Encoding.RegisterProvider method.
        protected async Task<string> GetResponseDataAsync(string url)
        {
            var bytes = await _httpClient.GetByteArrayAsync(url);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
