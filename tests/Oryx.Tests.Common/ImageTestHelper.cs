// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Globalization;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Tests.Common
{
    /// <summary>
    /// Helper class for operations involving images in Oryx test projects.
    /// </summary>
    public class ImageTestHelper
    {
        private const string _repoPrefixEnvironmentVariable = "ORYX_TEST_IMAGE_BASE";
        private const string _tagSuffixEnvironmentVariable = "ORYX_TEST_TAG_SUFFIX";
        private const string _defaultRepoPrefix = "oryxdevmcr.azurecr.io/public/oryx";
        private const string _restrictedPermissionsImageRepoPrefix = "oryxtests";

        private const string _azureFunctionsJamStack = "azfunc-jamstack";
        private const string _azureFunctionsJamStackBuster = "azfunc-jamstack-buster";
        private const string _azureFunctionsJamStackBullseye = "azfunc-jamstack-bullseye";
        private const string _gitHubActions = "github-actions";
        private const string _gitHubActionsBuster = "github-actions-buster";
        private const string _gitHubActionsBullseye = "github-actions-bullseye";
        private const string _gitHubActionsBase = "github-actions-base";
        private const string _gitHubActionsBusterBase = "github-actions-buster-base";
        private const string _gitHubActionsBullseyeBase = "github-actions-bullseye-base";
        private const string _gitHubActionsBaseWithEnv = "github-actions-base-withenv";
        private const string _gitHubActionsBusterBaseWithEnv = "github-actions-buster-base-withenv";
        private const string _gitHubActionsBullseyeBaseWithEnv = "github-actions-bullseye-base-withenv";
        private const string _vso = "vso";
        private const string _vsoUbuntu = "vso-focal";
        private const string _buildRepository = "build";
        private const string _packRepository = "pack";
        private const string _cliRepository = "cli";
        private const string _cliBusterRepository = "cli-buster";
        private const string _latestTag = "latest";
        private const string _ltsVersionsTag = "lts-versions";
        private const string _ltsVersionsBuster = "lts-versions-buster";
        private const string _full = "full";

        private readonly ITestOutputHelper _output;
        private string _repoPrefix;
        private string _tagSuffix;

        public ImageTestHelper() : this(output: null)
        {
        }

        public ImageTestHelper(ITestOutputHelper output)
        {
            _output = output;
            _repoPrefix = Environment.GetEnvironmentVariable(_repoPrefixEnvironmentVariable);
            if (string.IsNullOrEmpty(_repoPrefix))
            {
                // If the ORYX_TEST_IMAGE_BASE environment variable was not set in the .sh script calling this test,
                // then use the default value of 'oryxdevmcr.azurecr.io/public/oryx' as the image base for the tests.
                // This should be used in cases where a image base should be used for the tests rather than the
                // development registry (e.g., oryxmcr.azurecr.io/public/oryx)
                _output?.WriteLine($"Could not find a value for environment variable " +
                                  $"'{_repoPrefixEnvironmentVariable}', using default repo prefix '{_defaultRepoPrefix}'.");
                _repoPrefix = _defaultRepoPrefix;
            }

            _tagSuffix = Environment.GetEnvironmentVariable(_tagSuffixEnvironmentVariable);
            if (string.IsNullOrEmpty(_tagSuffix))
            {
                // If the ORYX_TEST_TAG_SUFFIX environment variable was not set in the .sh script calling this test,
                // then don't append a suffix to the tag of this image. This should be used in cases where a specific
                // runtime version tag should be used (e.g., node:8.8-20191025.1 instead of node:8.8)
                _output?.WriteLine($"Could not find a value for environment variable " +
                                  $"'{_tagSuffixEnvironmentVariable}', no suffix will be added to image tags.");
                _tagSuffix = string.Empty;
            }
        }

        /// <summary>
        /// NOTE: This constructor should only be used for ImageTestHelper unit tests.
        /// </summary>
        /// <param name="output">XUnit output helper for logging.</param>
        /// <param name="repoPrefix">The image base used to mimic the ORYX_TEST_IMAGE_BASE environment variable.</param>
        /// <param name="tagSuffix">The tag suffix used to mimic the ORYX_TEST_TAG_SUFFIX environment variable.</param>
        public ImageTestHelper(ITestOutputHelper output, string repoPrefix, string tagSuffix)
        {
            _output = output;
            if (string.IsNullOrEmpty(repoPrefix))
            {
                _output?.WriteLine($"No value provided for imageBase, using default repo prefix '{_defaultRepoPrefix}'.");
                repoPrefix = _defaultRepoPrefix;
            }

            _repoPrefix = repoPrefix;

            if (string.IsNullOrEmpty(tagSuffix))
            {
                _output?.WriteLine("No value provided for tagSuffix, no suffix will be added to image tags.");
            }

            _tagSuffix = tagSuffix;
        }

        /// <summary>
        /// Gets helper where the repoPrefix is set as <see cref="_restrictedPermissionsImageRepoPrefix"/> and tagSuffix
        /// is set to empty. The image names that are returned by this helper is for testing inside an image with
        /// restricted permissions.
        /// </summary>
        /// <returns></returns>
        public static ImageTestHelper WithRestrictedPermissions()
        {
            return WithRestrictedPermissions(outputHelper: null);
        }

        /// <summary>
        /// Gets helper where the repoPrefix is set as <see cref="_restrictedPermissionsImageRepoPrefix"/> and tagSuffix
        /// is set to empty. The image names that are returned by this helper is for testing inside an image with
        /// restricted permissions.
        /// </summary>
        /// <returns></returns>
        public static ImageTestHelper WithRestrictedPermissions(ITestOutputHelper outputHelper)
        {
            return new ImageTestHelper(
                output: outputHelper,
                repoPrefix: _restrictedPermissionsImageRepoPrefix,
                tagSuffix: string.Empty);
        }

        /// <summary>
        /// Constructs a runtime image from the given parameters that follows the format
        /// '{image}/{platformName}:{platformVersion}{tagSuffix}'. The base image can be set with the environment
        /// variable ORYX_TEST_IMAGE_BASE, otherwise the default base 'oryxdevmcr.azurecr.io/public/oryx' will be used.
        /// If a tag suffix was set with the environment variable ORYX_TEST_TAG_SUFFIX, it will be appended to the tag.
        /// </summary>
        /// <param name="platformName">The platform to pull the runtime image from.</param>
        /// <param name="platformVersion">The version of the platform to pull the runtime image from.</param>
        /// <returns>A runtime image that can be pulled for testing.</returns>
        public string GetRuntimeImage(string platformName, string platformVersion)
        {
            return $"{_repoPrefix}/{platformName}:{platformVersion}{_tagSuffix}";
        }

        /// <summary>
        /// Constructs a 'build' image using either the default image base (oryxdevmcr.azurecr.io/public/oryx), or the
        /// base set by the ORYX_TEST_IMAGE_BASE environment variable. If a tag suffix was set with the environment
        /// variable ORYX_TEST_TAG_SUFFIX, it will be used as the tag, otherwise, the 'latest' tag will be used.
        /// </summary>
        /// <returns>A 'build' image that can be pulled for testing.</returns>
        public string GetBuildImage()
        {
            var tag = GetTestTag();
            return $"{_repoPrefix}/{_buildRepository}:{tag}";
        }

        /// <summary>
        /// Constructs a 'build' or 'build:lts-versions' image based on the provided tag.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public string GetBuildImage(string tag)
        {
            if (string.Equals(tag, _latestTag))
            {
                return GetBuildImage();
            }
            else if (string.Equals(tag, _ltsVersionsTag))
            {
                return GetLtsVersionsBuildImage();
            }
            else if (string.Equals(tag, _vso))
            {
                return GetVsoBuildImage();
            }
            else if (string.Equals(tag, _vsoUbuntu))
            {
                return GetVsoBuildImage(_vsoUbuntu);
            }
            else if (string.Equals(tag, _gitHubActions))
            {
                return GetGitHubActionsBuildImage();
            }
            else if (string.Equals(tag, _gitHubActionsBuster))
            {
                return GetGitHubActionsBuildImage(_gitHubActionsBuster);
            }
            else if (string.Equals(tag, _gitHubActionsBullseye))
            {
                return GetGitHubActionsBuildImage(_gitHubActionsBullseye);
            }
            else if (string.Equals(tag, _ltsVersionsBuster))
            {
                return GetLtsVersionsBuildImage(_ltsVersionsBuster);
            }
            else if (string.Equals(tag, _azureFunctionsJamStack))
            {
                return GetAzureFunctionsJamStackBuildImage(_azureFunctionsJamStack);
            }
            else if (string.Equals(tag, _azureFunctionsJamStackBuster))
            {
                return GetAzureFunctionsJamStackBuildImage(_azureFunctionsJamStackBuster);
            }
            else if (string.Equals(tag, _azureFunctionsJamStackBullseye))
            {
                return GetAzureFunctionsJamStackBuildImage(_azureFunctionsJamStackBullseye);
            }
            else if (string.Equals(tag, _cliRepository))
            {
                return GetCliImage(_cliRepository);
            }
            else if (string.Equals(tag, _cliBusterRepository))
            {
                return GetCliImage(_cliBusterRepository);
            }
            else if (string.Equals(tag, _full))
            {
                return GetFullBuildImage();
            }

            throw new NotSupportedException($"A build image cannot be created with the given tag '{tag}'.");
        }

        /// <summary>
        /// Constructs a 'build:lts-versions' image using either the default image base (oryxdevmcr.azurecr.io/public/oryx), or the
        /// base set by the ORYX_TEST_IMAGE_BASE environment variable. If a tag suffix was set with the environment
        /// variable ORYX_TEST_TAG_SUFFIX, it will be used as the tag, otherwise, the 'latest' tag will be used.
        /// </summary>
        /// <returns>A 'build:slim' image that can be pulled for testing.</returns>
        public string GetLtsVersionsBuildImage()
        {
            return $"{_repoPrefix}/{_buildRepository}:{_ltsVersionsTag}{_tagSuffix}";
        }

        /// <summary>
        /// Constructs a 'build:slim' image using either the default image base (oryxdevmcr.azurecr.io/public/oryx), or the
        /// base set by the ORYX_TEST_IMAGE_BASE environment variable. If a tag suffix was set with the environment
        /// variable ORYX_TEST_TAG_SUFFIX, it will be used as the tag, otherwise, the 'latest' tag will be used.
        /// </summary>
        /// <returns>A 'build:slim' image that can be pulled for testing.</returns>
        public string GetAzureFunctionsJamStackBuildImage(string debianFlavor=null)
        {
            if (!string.IsNullOrEmpty(debianFlavor)
                && string.Equals(debianFlavor.ToLower(), _azureFunctionsJamStackBuster))
            {
                return $"{_repoPrefix}/{_buildRepository}:{_azureFunctionsJamStackBuster}{_tagSuffix}";
            } else if (!string.IsNullOrEmpty(debianFlavor) && string.Equals(debianFlavor.ToLower(), _azureFunctionsJamStackBullseye)) {
                return $"{_repoPrefix}/{_buildRepository}:{_azureFunctionsJamStackBullseye}{_tagSuffix}";
            } else {
                return $"{_repoPrefix}/{_buildRepository}:{_azureFunctionsJamStack}{_tagSuffix}";
            }
        }

        public string GetGitHubActionsBuildImage(string debianFlavor=null)
        {
            if (!string.IsNullOrEmpty(debianFlavor) && string.Equals(debianFlavor.ToLower(), _gitHubActionsBuster)) {
                return $"{_repoPrefix}/{_buildRepository}:{_gitHubActionsBuster}{_tagSuffix}";
            } else if (!string.IsNullOrEmpty(debianFlavor) && string.Equals(debianFlavor.ToLower(), _gitHubActionsBullseye)) {
                return $"{_repoPrefix}/{_buildRepository}:{_gitHubActionsBullseye}{_tagSuffix}";
            } else {
                return $"{_repoPrefix}/{_buildRepository}:{_gitHubActions}{_tagSuffix}";
            }
        }

        public string GetGitHubActionsAsBaseBuildImage(string debianFlavor = null)
        {
            if (!string.IsNullOrEmpty(debianFlavor) && string.Equals(debianFlavor.ToLower(), _gitHubActionsBusterBase))
            {
                return $"{_restrictedPermissionsImageRepoPrefix}/{_buildRepository}:{_gitHubActionsBusterBase}";
            }
            else if (!string.IsNullOrEmpty(debianFlavor) && string.Equals(debianFlavor.ToLower(), _gitHubActionsBullseyeBase))
            {
                return $"{_restrictedPermissionsImageRepoPrefix}/{_buildRepository}:{_gitHubActionsBullseyeBase}";
            }
            else
            {
                return $"{_restrictedPermissionsImageRepoPrefix}/{_buildRepository}:{_gitHubActionsBase}";
            }
        }

        public string GetGitHubActionsAsBaseWithEnvBuildImage(string debianFlavor = null)
        {
            if (!string.IsNullOrEmpty(debianFlavor) && string.Equals(debianFlavor.ToLower(), _gitHubActionsBusterBaseWithEnv))
            {
                return $"{_restrictedPermissionsImageRepoPrefix}/{_buildRepository}:{_gitHubActionsBusterBaseWithEnv}";
            }
            else if (!string.IsNullOrEmpty(debianFlavor) && string.Equals(debianFlavor.ToLower(), _gitHubActionsBullseyeBaseWithEnv))
            {
                return $"{_restrictedPermissionsImageRepoPrefix}/{_buildRepository}:{_gitHubActionsBullseyeBaseWithEnv}";
            }
            else
            {
                return $"{_restrictedPermissionsImageRepoPrefix}/{_buildRepository}:{_gitHubActionsBaseWithEnv}";
            }
        }

        public string GetVsoBuildImage(string debianFlavor=null)
        {
            return $"{_repoPrefix}/{_buildRepository}:{_vsoUbuntu}{_tagSuffix}";
        }

        public string GetLtsVersionsBuildImage(string debianFlavor = null)
        {
            if (!string.IsNullOrEmpty(debianFlavor)
                && string.Equals(debianFlavor.ToLower(), _ltsVersionsBuster))
            {
                return $"{_repoPrefix}/{_buildRepository}:{_ltsVersionsBuster}{_tagSuffix}";
            }
            return $"{_repoPrefix}/{_buildRepository}:{_gitHubActions}{_tagSuffix}";
        }

        /// <summary>
        /// Constructs a 'pack' image using either the default image base (oryxdevmcr.azurecr.io/public/oryx), or the
        /// base set by the ORYX_TEST_IMAGE_BASE environment variable. If a tag suffix was set with the environment
        /// variable ORYX_TEST_TAG_SUFFIX, it will be used as the tag, otherwise, the 'latest' tag will be used.
        /// </summary>
        /// <returns>A 'pack' image that can be pulled for testing.</returns>
        public string GetPackImage()
        {
            var tag = GetTestTag();
            return $"{_repoPrefix}/{_packRepository}:{tag}";
        }

        /// <summary>
        /// Constructs a 'cli' image using either the default image base (oryxdevmcr.azurecr.io/public/oryx), or the
        /// base set by the ORYX_TEST_IMAGE_BASE environment variable. If a tag suffix was set with the environment
        /// variable ORYX_TEST_TAG_SUFFIX, it will be used as the tag, otherwise, the 'latest' tag will be used.
        /// </summary>
        /// <returns>A 'cli' image that can be pulled for testing.</returns>
        public string GetCliImage(string debianFlavor = null)
        {
            var tag = GetTestTag();
            if (!string.IsNullOrEmpty(debianFlavor)
                && string.Equals(debianFlavor.ToLower(), _cliBusterRepository))
            {
                return $"{_repoPrefix}/{_cliBusterRepository}:{tag}";
            }

            return $"{_repoPrefix}/{_cliRepository}:{tag}";
        }

        private string GetTestTag()
        {
            if (string.IsNullOrEmpty(_tagSuffix))
            {
                return _latestTag;
            }

            if (_tagSuffix.StartsWith("-"))
            {
                return _tagSuffix.TrimStart('-');
            }

            return _tagSuffix;
        }

        private string GetFullBuildImage()
        {
            return $"{_repoPrefix}/{_buildRepository}:{_full}";
        }
    }
}
