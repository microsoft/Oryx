// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Tests.Common
{
    /// <summary>
    /// Helper class for operations involving images in Oryx test projects.
    /// </summary>
    public class ImageTestHelper
    {
        private const string _registryNameEnvironmentVariable = "ORYX_TEST_REGISTRY_NAME";
        private const string _tagSuffixEnvironmentVariable = "ORYX_TEST_TAG_SUFFIX";
        private const string _defaultRegistryName = "oryxdevmcr.azurecr.io";
        private const string _defaultRepositoryPrefix = "/public/oryx";

        private readonly ITestOutputHelper _output;
        private string _image;
        private string _tagSuffix;

        public ImageTestHelper(ITestOutputHelper output)
        {
            _output = output;
            _image = Environment.GetEnvironmentVariable(_registryNameEnvironmentVariable);
            if (string.IsNullOrEmpty(_image))
            {
                // If the ORYX_TEST_REGISTRY_NAME environment variable was not set in the .sh script calling this test,
                // then use the default value of 'oryxdevmcr.azurecr.io' as the container registry for the tests. This
                // should be used in cases where a specific registry should be used for the tests rather than the
                // development registry (e.g., oryxmcr.azurecr.io)
                _output.WriteLine($"Could not find a value for environment variable " +
                                  $"'{_registryNameEnvironmentVariable}', using default container registry '{_defaultRegistryName}'.");
                _image = _defaultRegistryName;
            }

            _image += _defaultRepositoryPrefix;

            _tagSuffix = Environment.GetEnvironmentVariable(_tagSuffixEnvironmentVariable);
            if (string.IsNullOrEmpty(_tagSuffix))
            {
                // If the ORYX_TEST_TAG_SUFFIX environment variable was not set in the .sh script calling this test,
                // then don't append a suffix to the tag of this image. This should be used in cases where a specific
                // runtime version tag should be used (e.g., node:8.8-20191025.1 instead of node:8.8)
                _output.WriteLine($"Could not find a value for environment variable " +
                                  $"'{_tagSuffixEnvironmentVariable}', not suffix will be added to image tags.");
                _tagSuffix = string.Empty;
            }
        }

        /// <summary>
        /// Constructs a runtime image from the given parameters that follows the format
        /// {image}/{platform}:{platformVersion}. Since no image is provided, the default image 'oryxdevmcr.azurecr.io',
        /// or any image set by environment variable will be used. If any tag suffix was set as an environment variable,
        /// it will be appended to the end of the tag.
        /// </summary>
        /// <param name="platform">The platform to pull the runtime image from.</param>
        /// <param name="platformVersion">The version of the platform to pull the runtime image from.</param>
        /// <returns>A runtime image that can be pulled for testing.</returns>
        public string GetRuntimeImage(string platform, string platformVersion)
        {
            return $"{_image}/{platform}:{platformVersion}{_tagSuffix}";
        }
    }
}
