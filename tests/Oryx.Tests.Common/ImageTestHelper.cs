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
        private const string _imageBaseEnvironmentVariable = "ORYX_TEST_IMAGE_BASE";
        private const string _tagSuffixEnvironmentVariable = "ORYX_TEST_TAG_SUFFIX";
        private const string _defaultImageBase = "oryxdevmcr.azurecr.io/public/oryx";

        private readonly ITestOutputHelper _output;
        private string _image;
        private string _tagSuffix;

        public ImageTestHelper(ITestOutputHelper output)
        {
            _output = output;
            _image = Environment.GetEnvironmentVariable(_imageBaseEnvironmentVariable);
            if (string.IsNullOrEmpty(_image))
            {
                // If the ORYX_TEST_IMAGE_BASE environment variable was not set in the .sh script calling this test,
                // then use the default value of 'oryxdevmcr.azurecr.io/public/oryx' as the image base for the tests.
                // This should be used in cases where a image base should be used for the tests rather than the
                // development registry (e.g., oryxmcr.azurecr.io/public/oryx)
                _output.WriteLine($"Could not find a value for environment variable " +
                                  $"'{_imageBaseEnvironmentVariable}', using default image base '{_defaultImageBase}'.");
                _image = _defaultImageBase;
            }

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
        /// Constructs an image from the given parameters that follows the format '{imageBase}/{repositoryName}:{tag}'.
        /// The default image base used is 'oryxdevmcr.azurecr.io/public/oryx'; the image base can be set manually by
        /// assigning a value to the corresponding environment variable (see constructor). If any tag suffix was set
        /// as an environment variable, it will be appended to the end of the given tag.
        /// </summary>
        /// <param name="repositoryName">The name of the repository to pull from (e.g., 'build', 'python', 'pack', etc.).</param>
        /// <param name="tag">The name of the tag to pull from; if there is a tag suffix, it will be appended.</param>
        /// <returns>An image that can be pulled for testing.</returns>
        public string GetTestImage(string repositoryName, string tag)
        {
            return $"{_image}/{repositoryName}:{tag}{_tagSuffix}";
        }

        /// <summary>
        /// Constructs an image from the given parameters that follows the format '{imageBase}/{repositoryName}:{tag}'.
        /// The default image base used is 'oryxdevmcr.azurecr.io/public/oryx'; the image base can be set manually by
        /// assigning a value to the corresponding environment variable (see constructor). If any tag suffix was set
        /// as an environment variable, it will be used as the tag, otherwise 'latest' will be used.
        /// </summary>
        /// <param name="repositoryName">The name of the repository to pull from (e.g., 'build', 'python', 'pack', etc.).</param>
        /// <returns>An image that can be pulled for testing.</returns>
        public string GetTestImage(string repositoryName)
        {
            var tag = string.IsNullOrEmpty(_tagSuffix) ? "latest" : _tagSuffix;
            return $"{_image}/{repositoryName}:{tag}";
        }
    }
}
