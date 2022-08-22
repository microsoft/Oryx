// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.BuildScriptGenerator.Php;
using Microsoft.Oryx.BuildScriptGenerator.Python;
using Microsoft.Oryx.BuildScriptGenerator.Ruby;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Tests.Common
{
    /// <summary>
    /// Helper class for operations involving images in Oryx test projects.
    /// </summary>
    public class ImageTestHelper
    {
        private const string _repoPrefixEnvironmentVariable = ImageTestHelperConstants.RepoPrefixEnvironmentVariable;
        private const string _tagSuffixEnvironmentVariable = ImageTestHelperConstants.TagSuffixEnvironmentVariable;
        private const string _defaultRepoPrefix = ImageTestHelperConstants.DefaultRepoPrefix;
        private const string _restrictedPermissionsImageRepoPrefix = ImageTestHelperConstants.RestrictedPermissionsImageRepoPrefix;

        private const string _azureFunctionsJamStackStretch = ImageTestHelperConstants.AzureFunctionsJamStackStretch;
        private const string _azureFunctionsJamStackBuster = ImageTestHelperConstants.AzureFunctionsJamStackBuster;
        private const string _azureFunctionsJamStackBullseye = ImageTestHelperConstants.AzureFunctionsJamStackBullseye;
        private const string _gitHubActionsStretch = ImageTestHelperConstants.GitHubActionsStretch;
        private const string _gitHubActionsBuster = ImageTestHelperConstants.GitHubActionsBuster;
        private const string _gitHubActionsBullseye = ImageTestHelperConstants.GitHubActionsBullseye;
        private const string _vso = ImageTestHelperConstants.Vso;
        private const string _vsoUbuntu = ImageTestHelperConstants.VsoUbuntu;
        private const string _buildRepository = ImageTestHelperConstants.BuildRepository;
        private const string _packRepository = ImageTestHelperConstants.PackRepository;
        private const string _cliRepository = ImageTestHelperConstants.CliRepository;
        private const string _cliBusterRepository = ImageTestHelperConstants.CliBusterRepository;
        private const string _cliStretchTag = ImageTestHelperConstants.CliStretchTag;
        private const string _cliBusterTag = ImageTestHelperConstants.CliBusterTag;
        private const string _latestTag = ImageTestHelperConstants.LatestTag;
        private const string _ltsVersionsStretch = ImageTestHelperConstants.LtsVersionsStretch;
        private const string _ltsVersionsBuster = ImageTestHelperConstants.LtsVersionsBuster;

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
        /// '{image}/{platformName}:{platformVersion}-{osType}-{osFlavor}{tagSuffix}'. The base image can be set with the environment
        /// variable ORYX_TEST_IMAGE_BASE, otherwise the default base 'oryxdevmcr.azurecr.io/public/oryx' will be used.
        /// If a tag suffix was set with the environment variable ORYX_TEST_TAG_SUFFIX, it will be appended to the tag.
        /// </summary>
        /// <param name="platformName">The platform to pull the runtime image from.</param>
        /// <param name="platformVersion">The version of the platform to pull the runtime image from.</param>
        /// <returns>A runtime image that can be pulled for testing.</returns>
        public string GetRuntimeImage(string platformName, string platformVersion)
        {

            if (PlatformVersionToOsType.TryGetValue(platformName, out var versionToOsType) 
                && versionToOsType.TryGetValue(platformVersion, out var osType))
            {
                return $"{_repoPrefix}/{platformName}:{platformVersion}-{osType}{_tagSuffix}";
            }
            
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
            else if (string.Equals(tag, _ltsVersionsStretch))
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
            else if (string.Equals(tag, _gitHubActionsStretch))
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
            else if (string.Equals(tag, _azureFunctionsJamStackStretch))
            {
                return GetAzureFunctionsJamStackBuildImage(_azureFunctionsJamStackStretch);
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
            return $"{_repoPrefix}/{_buildRepository}:{_ltsVersionsStretch}{_tagSuffix}";
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
                return $"{_repoPrefix}/{_buildRepository}:{_azureFunctionsJamStackStretch}{_tagSuffix}";
            }
        }

        public string GetGitHubActionsBuildImage(string debianFlavor=null)
        {
            if (!string.IsNullOrEmpty(debianFlavor) && string.Equals(debianFlavor.ToLower(), _gitHubActionsBuster)) {
                return $"{_repoPrefix}/{_buildRepository}:{_gitHubActionsBuster}{_tagSuffix}";
            } else if (!string.IsNullOrEmpty(debianFlavor) && string.Equals(debianFlavor.ToLower(), _gitHubActionsBullseye)) {
                return $"{_repoPrefix}/{_buildRepository}:{_gitHubActionsBullseye}{_tagSuffix}";
            } else {
                return $"{_repoPrefix}/{_buildRepository}:{_gitHubActionsStretch}{_tagSuffix}";
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
            return $"{_repoPrefix}/{_buildRepository}:{_gitHubActionsStretch}{_tagSuffix}";
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
            if (!string.IsNullOrEmpty(debianFlavor)
                && string.Equals(debianFlavor.ToLower(), _cliBusterRepository))
            {
                return $"{_repoPrefix}/{_cliBusterRepository}:{_cliBusterTag}{_tagSuffix}";
            }

            return $"{_repoPrefix}/{_cliRepository}:{_cliStretchTag}{_tagSuffix}";
        }

        private string GetTestTag()
        {
            if (string.IsNullOrEmpty(_tagSuffix))
            {
                return _latestTag;
            }

            if (_tagSuffix.StartsWith("-"))
            {
                return $"{_latestTag}{_tagSuffix}";
            }

            return $"{_latestTag}-{_tagSuffix}";
        }

        private Dictionary<string, Dictionary<string, string>> PlatformVersionToOsType = new Dictionary<string, Dictionary<string, string>>
        {
            {
                DotNetCoreConstants.RuntimePlatformName,
                new Dictionary<string, string>
                {
                    { "3.0", "debian-buster" },
                    { "3.1", "debian-bullseye" },
                    { "5.0", "debian-buster" },
                    { "6.0", "debian-buster" },
                    { "7.0", "debian-buster" },
                    { "dynamic", "debian-buster" },
                }
            },
            {
                NodeConstants.NodeToolName,
                new Dictionary<string, string>
                {
                    { "14", "debian-bullseye" },
                    { "16", "debian-bullseye" },
                    { "dynamic", "debian-buster" },
                }
            },
            {
                PhpConstants.PlatformName,
                new Dictionary<string, string>
                {
                    { "7.4", "debian-bullseye" },
                    { "8.0", "debian-buster" },
                    { "8.1", "debian-bullseye" },
                    { "7.4-fpm", "debian-bullseye" },
                    { "8.0-fpm", "debian-buster" },
                    { "8.1-fpm", "debian-bullseye" },
                }
            },
            {
                PythonConstants.PlatformName,
                new Dictionary<string, string>
                {
                    { "3.7", "debian-bullseye" },
                    { "3.8", "debian-bullseye" },
                    { "3.9", "debian-buster" },
                    { "3.10", "debian-bullseye" },
                    { "dynamic", "debian-buster" },
                }
            },
            {
                RubyConstants.PlatformName,
                new Dictionary<string, string>
                {
                    { "2.5", "debian-buster" },
                    { "2.6", "debian-buster" },
                    { "2.7", "debian-buster" },
                    { "dynamic", "debian-buster" },
                }
            },
        };
    }

    public static class ImageTestHelperConstants 
    {
        public const string RepoPrefixEnvironmentVariable = "ORYX_TEST_IMAGE_BASE";
        public const string TagSuffixEnvironmentVariable = "ORYX_TEST_TAG_SUFFIX";
        public const string DefaultRepoPrefix = "oryxdevmcr.azurecr.io/public/oryx";
        public const string RestrictedPermissionsImageRepoPrefix = "oryxtests";

        public const string AzureFunctionsJamStackStretch = "azfunc-jamstack-debian-stretch";
        public const string AzureFunctionsJamStackBuster = "azfunc-jamstack-debian-buster";
        public const string AzureFunctionsJamStackBullseye = "azfunc-jamstack-debian-bullseye";
        public const string GitHubActionsStretch = "github-actions-debian-stretch";
        public const string GitHubActionsBuster = "github-actions-debian-buster";
        public const string GitHubActionsBullseye = "github-actions-debian-bullseye";
        public const string Vso = "vso";
        public const string VsoUbuntu = "ubuntu-vso-focal";
        public const string BuildRepository = "build";
        public const string PackRepository = "pack";
        public const string CliRepository = "cli";
        public const string CliBusterRepository = "cli-buster";
        public const string CliStretchTag = "debian-stretch";
        public const string CliBusterTag = "debian-buster";
        public const string LatestTag = "debian-stretch";
        public const string LtsVersionsStretch = "lts-versions-debian-stretch";
        public const string LtsVersionsBuster = "lts-versions-debian-buster";
    }
}
