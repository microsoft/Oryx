// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.BuildScriptGenerator.Php;
using Microsoft.Oryx.BuildScriptGenerator.Python;
using Microsoft.Oryx.BuildScriptGenerator.Ruby;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
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
        private const string _defaultStagingRepoPrefix = ImageTestHelperConstants.DefaultStagingRepoPrefix;
        private const string _restrictedPermissionsImageRepoPrefix = ImageTestHelperConstants.RestrictedPermissionsImageRepoPrefix;

        private const string _azureFunctionsJamStackStretch = ImageTestHelperConstants.AzureFunctionsJamStackStretch;
        private const string _azureFunctionsJamStackBuster = ImageTestHelperConstants.AzureFunctionsJamStackBuster;
        private const string _azureFunctionsJamStackBullseye = ImageTestHelperConstants.AzureFunctionsJamStackBullseye;
        private const string _gitHubActionsStretch = ImageTestHelperConstants.GitHubActionsStretch;
        private const string _gitHubActionsBuster = ImageTestHelperConstants.GitHubActionsBuster;
        private const string _gitHubActionsBullseye = ImageTestHelperConstants.GitHubActionsBullseye;
        private const string _gitHubActionsBookworm = ImageTestHelperConstants.GitHubActionsBookworm;
        private const string _gitHubActionsStretchBase = ImageTestHelperConstants.GitHubActionsStretchBase;
        private const string _gitHubActionsBusterBase = ImageTestHelperConstants.GitHubActionsBusterBase;
        private const string _gitHubActionsBullseyeBase = ImageTestHelperConstants.GitHubActionsBullseyeBase;
        private const string _gitHubActionsBookwormBase = ImageTestHelperConstants.GitHubActionsBookwormBase;
        private const string _gitHubActionsStretchBaseWithEnv = ImageTestHelperConstants.GitHubActionsStretchBaseWithEnv;
        private const string _gitHubActionsBusterBaseWithEnv = ImageTestHelperConstants.GitHubActionsBusterBaseWithEnv;
        private const string _gitHubActionsBullseyeBaseWithEnv = ImageTestHelperConstants.GitHubActionsBullseyeBaseWithEnv;
        private const string _gitHubActionsBookwormBaseWithEnv = ImageTestHelperConstants.GitHubActionsBookwormBaseWithEnv;
        private const string _vso = ImageTestHelperConstants.Vso;
        private const string _vsoUbuntu = ImageTestHelperConstants.VsoFocal;
        private const string _vsoBullseye = ImageTestHelperConstants.VsoBullseye;
        private const string _buildRepository = ImageTestHelperConstants.BuildRepository;
        private const string _packRepository = ImageTestHelperConstants.PackRepository;
        private const string _cliRepository = ImageTestHelperConstants.CliRepository;
        private const string _cliStretchTag = ImageTestHelperConstants.CliStretchTag;
        private const string _cliBusterTag = ImageTestHelperConstants.CliBusterTag;
        private const string _cliBullseyeTag = ImageTestHelperConstants.CliBullseyeTag;
        private const string _cliBuilderBullseyeTag = ImageTestHelperConstants.CliBuilderBullseyeTag;
        private const string _latestTag = ImageTestHelperConstants.LatestStretchTag;
        private const string _ltsVersionsStretch = ImageTestHelperConstants.LtsVersionsStretch;
        private const string _ltsVersionsBuster = ImageTestHelperConstants.LtsVersionsBuster;
        private const string _fullStretch = ImageTestHelperConstants.FullStretch;
        private const string _fullBullseye = ImageTestHelperConstants.FullBullseye;
        private const string _fullBuster = ImageTestHelperConstants.FullBuster;

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
        /// <param name="osType">The OS type of the runtime image to use.</param>
        /// <returns>A runtime image that can be pulled for testing.</returns>
        public string GetRuntimeImage(string platformName, string platformVersion, string osType)
        {
            var runtimeRepoPrefix = _repoPrefix;

            // if this platform and version are marked as staging, replace the public repo with the staging repo
            switch (platformName)
            {
                case DotNetCoreConstants.RuntimePlatformName:
                    if (StagingRuntimeConstants.DotnetcoreStagingRuntimeVersions.Contains(platformVersion))
                    {
                        runtimeRepoPrefix = runtimeRepoPrefix.Replace(_defaultRepoPrefix, _defaultStagingRepoPrefix);
                    }
                    break;
            }

            var runtimeImageTag = $"{platformVersion}-{osType}";
            if (PlatformVersionToOsType.TryGetValue(platformName, out var versionToOsType)
                && versionToOsType.Contains(runtimeImageTag))
            {
                return $"{runtimeRepoPrefix}/{platformName}:{platformVersion}-{osType}{_tagSuffix}";
            }

            if (PlatformVersionToOsType.TryGetValue(platformName, out versionToOsType)
                && versionToOsType.Any(v => v.StartsWith(platformVersion)))
            {
                osType = versionToOsType
                    .First(v => v.StartsWith(platformVersion))
                    .Substring(platformVersion.Length + 1);
                return $"{runtimeRepoPrefix}/{platformName}:{platformVersion}-{osType}{_tagSuffix}";
            }

            return $"{runtimeRepoPrefix}/{platformName}:{platformVersion}{_tagSuffix}";
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
            else if (string.Equals(tag, _vsoBullseye))
            {
                return GetVsoBuildImage(_vsoBullseye);
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
            else if (string.Equals(tag, _gitHubActionsBookworm))
            {
                return GetGitHubActionsBuildImage(_gitHubActionsBookworm);
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
            else if (string.Equals(tag, _cliBusterTag))
            {
                return GetCliImage(_cliBusterTag);
            }
            else if(string.Equals(tag, _cliBullseyeTag))
            {
                return GetCliImage(_cliBullseyeTag);
            }
            else if(string.Equals(tag, _cliBuilderBullseyeTag))
            {
                return GetCliBuilderImage(_cliBuilderBullseyeTag);
            }
            else if (string.Equals(tag, _fullStretch))
            {
                return GetFullBuildImage(_fullStretch);
            }
            else if (string.Equals(tag, _fullBullseye))
            {
                return GetFullBuildImage(_fullBullseye);
            }
            else if (string.Equals(tag, _fullBuster))
            {
                return GetFullBuildImage(_fullBuster);
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
        public string GetAzureFunctionsJamStackBuildImage(string buildImageTag = null)
        {
            if (!string.IsNullOrEmpty(buildImageTag)
                && string.Equals(buildImageTag.ToLower(), _azureFunctionsJamStackBuster))
            {
                return $"{_repoPrefix}/{_buildRepository}:{_azureFunctionsJamStackBuster}{_tagSuffix}";
            }
            else if (!string.IsNullOrEmpty(buildImageTag) && string.Equals(buildImageTag.ToLower(), _azureFunctionsJamStackBullseye))
            {
                return $"{_repoPrefix}/{_buildRepository}:{_azureFunctionsJamStackBullseye}{_tagSuffix}";
            }
            else
            {
                return $"{_repoPrefix}/{_buildRepository}:{_azureFunctionsJamStackStretch}{_tagSuffix}";
            }
        }

        public string GetGitHubActionsBuildImage(string buildImageTag = null)
        {
            if (!string.IsNullOrEmpty(buildImageTag) && string.Equals(buildImageTag.ToLower(), _gitHubActionsBuster))
            {
                return $"{_repoPrefix}/{_buildRepository}:{_gitHubActionsBuster}{_tagSuffix}";
            }
            else if (!string.IsNullOrEmpty(buildImageTag) && string.Equals(buildImageTag.ToLower(), _gitHubActionsBookworm))
            {
                return $"{_repoPrefix}/{_buildRepository}:{_gitHubActionsBookworm}{_tagSuffix}";
            }
            else if (!string.IsNullOrEmpty(buildImageTag) && string.Equals(buildImageTag.ToLower(), _gitHubActionsBullseye))
            {
                return $"{_repoPrefix}/{_buildRepository}:{_gitHubActionsBullseye}{_tagSuffix}";
            }
            else
            {
                return $"{_repoPrefix}/{_buildRepository}:{_gitHubActionsStretch}{_tagSuffix}";
            }
        }

        public string GetGitHubActionsAsBaseBuildImage(string buildImageTag = null)
        {
            if (!string.IsNullOrEmpty(buildImageTag) && string.Equals(buildImageTag.ToLower(), _gitHubActionsBookwormBase))
            {
                return $"{_restrictedPermissionsImageRepoPrefix}/{_buildRepository}:{_gitHubActionsBookwormBase}";
            }
            else if (!string.IsNullOrEmpty(buildImageTag) && string.Equals(buildImageTag.ToLower(), _gitHubActionsBusterBase))
            {
                return $"{_restrictedPermissionsImageRepoPrefix}/{_buildRepository}:{_gitHubActionsBusterBase}";
            }
            else if (!string.IsNullOrEmpty(buildImageTag) && string.Equals(buildImageTag.ToLower(), _gitHubActionsBullseyeBase))
            {
                return $"{_restrictedPermissionsImageRepoPrefix}/{_buildRepository}:{_gitHubActionsBullseyeBase}";
            }
            else
            {
                return $"{_restrictedPermissionsImageRepoPrefix}/{_buildRepository}:{_gitHubActionsStretchBase}";
            }
        }

        public string GetGitHubActionsAsBaseWithEnvBuildImage(string buildImageTag = null)
        {
            if (!string.IsNullOrEmpty(buildImageTag) && string.Equals(buildImageTag.ToLower(), _gitHubActionsBookwormBaseWithEnv))
            {
                return $"{_restrictedPermissionsImageRepoPrefix}/{_buildRepository}:{_gitHubActionsBookwormBaseWithEnv}";
            }
            else if (!string.IsNullOrEmpty(buildImageTag) && string.Equals(buildImageTag.ToLower(), _gitHubActionsBusterBaseWithEnv))
            {
                return $"{_restrictedPermissionsImageRepoPrefix}/{_buildRepository}:{_gitHubActionsBusterBaseWithEnv}";
            }
            else if (!string.IsNullOrEmpty(buildImageTag) && string.Equals(buildImageTag.ToLower(), _gitHubActionsBullseyeBaseWithEnv))
            {
                return $"{_restrictedPermissionsImageRepoPrefix}/{_buildRepository}:{_gitHubActionsBullseyeBaseWithEnv}";
            }
            else
            {
                return $"{_restrictedPermissionsImageRepoPrefix}/{_buildRepository}:{_gitHubActionsStretchBaseWithEnv}";
            }
        }

        public string GetVsoBuildImage(string buildImageTag = null)
        {
            if (!string.IsNullOrEmpty(buildImageTag)
                && string.Equals(buildImageTag.ToLower(), _vsoBullseye))
            {
                return $"{_repoPrefix}/{_buildRepository}:{_vsoBullseye}{_tagSuffix}";
            }
            return $"{_repoPrefix}/{_buildRepository}:{_vsoUbuntu}{_tagSuffix}";
        }

        public string GetLtsVersionsBuildImage(string buildImageTag = null)
        {
            if (!string.IsNullOrEmpty(buildImageTag)
                && string.Equals(buildImageTag.ToLower(), _ltsVersionsBuster))
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
        public string GetCliImage(string buildImageTag = null)
        {
            if (!string.IsNullOrEmpty(buildImageTag)
                && string.Equals(buildImageTag.ToLower(), _cliBusterTag))
            {
                return $"{_repoPrefix}/{_cliRepository}:{_cliBusterTag}{_tagSuffix}";
            }
            else if (!string.IsNullOrEmpty(buildImageTag) && string.Equals(buildImageTag.ToLower(), _cliBullseyeTag))
            {
                return $"{_repoPrefix}/{_cliRepository}:{_cliBullseyeTag}{_tagSuffix}";
            }

            return $"{_repoPrefix}/{_cliRepository}:{_cliStretchTag}{_tagSuffix}";
        }

        /// <summary>
        /// Constructs a 'cli' image using either the default image base (oryxdevmcr.azurecr.io/public/oryx), or the
        /// base set by the ORYX_TEST_IMAGE_BASE environment variable. If a tag suffix was set with the environment
        /// variable ORYX_TEST_TAG_SUFFIX, it will be used as the tag, otherwise, the 'latest' tag will be used.
        /// </summary>
        /// <returns>A 'cli builder' image that can be pulled for testing.</returns>
        public string GetCliBuilderImage(string imageTagPrefix = null)
        {
            if (!string.IsNullOrEmpty(imageTagPrefix)
                && string.Equals(imageTagPrefix.ToLower(), _cliBuilderBullseyeTag))
            {
                return $"{_repoPrefix}/{_cliRepository}:{_cliBuilderBullseyeTag}{_tagSuffix}";
            }

            throw new ArgumentException($"Could not find cli builder image with image tag prefix '{imageTagPrefix}'.");
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

        private string GetFullBuildImage(string buildImageTag = null)
        {
            if (!string.IsNullOrEmpty(buildImageTag) && string.Equals(buildImageTag.ToLower(), _fullBuster))
            {
                return $"{_repoPrefix}/{_buildRepository}:{_fullBuster}{_tagSuffix}";
            }
            else if (!string.IsNullOrEmpty(buildImageTag) && string.Equals(buildImageTag.ToLower(), _fullBullseye))
            {
                return $"{_repoPrefix}/{_buildRepository}:{_fullBullseye}{_tagSuffix}";
            }
            else
            {
                return $"{_repoPrefix}/{_buildRepository}:{_fullStretch}{_tagSuffix}";
            }
        }

        private Dictionary<string, List<string>> PlatformVersionToOsType = new Dictionary<string, List<string>>
        {
            {
                DotNetCoreConstants.RuntimePlatformName,
                DotNetCoreSdkVersions.RuntimeVersions
            },
            {
                NodeConstants.NodeToolName,
                NodeVersions.RuntimeVersions
            },
            {
                PhpConstants.PlatformName,
                PhpVersions.RuntimeVersions
                .Concat(PhpVersions.FpmRuntimeVersions)
                .ToList()
            },
            {
                PythonConstants.PlatformName,
                PythonVersions.RuntimeVersions
            },
            {
                RubyConstants.PlatformName,
                RubyVersions.RuntimeVersions
            },
        };
    }

    public static class ImageTestHelperConstants
    {
        public const string RepoPrefixEnvironmentVariable = "ORYX_TEST_IMAGE_BASE";
        public const string TagSuffixEnvironmentVariable = "ORYX_TEST_TAG_SUFFIX";
        public const string DefaultRepoPrefix = "oryxdevmcr.azurecr.io/public/oryx";
        public const string DefaultStagingRepoPrefix = "oryxdevmcr.azurecr.io/staging/oryx";
        public const string RestrictedPermissionsImageRepoPrefix = "oryxtests";

        public const string OsTypeDebianStretch = "debian-stretch";
        public const string OsTypeDebianBuster = "debian-buster";
        public const string OsTypeDebianBullseye = "debian-bullseye";
        public const string OsTypeDebianBookworm = "debian-bookworm";

        public const string AzureFunctionsJamStackStretch = "azfunc-jamstack-debian-stretch";
        public const string AzureFunctionsJamStackBuster = "azfunc-jamstack-debian-buster";
        public const string AzureFunctionsJamStackBullseye = "azfunc-jamstack-debian-bullseye";
        public const string GitHubActionsStretch = "github-actions-debian-stretch";
        public const string GitHubActionsBuster = "github-actions-debian-buster";
        public const string GitHubActionsBullseye = "github-actions-debian-bullseye";
        public const string GitHubActionsBookworm = "github-actions-debian-bookworm";
        public const string GitHubActionsStretchBase = "github-actions-debian-stretch-base";
        public const string GitHubActionsBusterBase = "github-actions-debian-buster-base";
        public const string GitHubActionsBullseyeBase = "github-actions-debian-bullseye-base";
        public const string GitHubActionsBookwormBase = "github-actions-debian-bookworm-base";
        public const string GitHubActionsStretchBaseWithEnv = "github-actions-debian-stretch-base-withenv";
        public const string GitHubActionsBusterBaseWithEnv = "github-actions-debian-buster-base-withenv";
        public const string GitHubActionsBullseyeBaseWithEnv = "github-actions-debian-bullseye-base-withenv";
        public const string GitHubActionsBookwormBaseWithEnv = "github-actions-debian-bookworm-base-withenv";
        public const string Vso = "vso";
        public const string VsoFocal = "vso-ubuntu-focal";
        public const string VsoBullseye = "vso-debian-bullseye";
        public const string BuildRepository = "build";
        public const string PackRepository = "pack";
        public const string CliRepository = "cli";
        public const string CliStretchTag = "debian-stretch";
        public const string CliBusterTag = "debian-buster";
        public const string CliBullseyeTag = "debian-bullseye";
        public const string CliBuilderBullseyeTag = "builder-debian-bullseye";
        public const string LatestStretchTag = "debian-stretch";
        public const string LtsVersionsStretch = "lts-versions-debian-stretch";
        public const string LtsVersionsBuster = "lts-versions-debian-buster";
        public const string FullStretch = "full-debian-stretch";
        public const string FullBuster = "full-debian-buster";
        public const string FullBullseye = "full-debian-bullseye";
    }
}