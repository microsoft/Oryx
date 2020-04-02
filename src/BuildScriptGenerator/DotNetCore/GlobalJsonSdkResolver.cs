// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    public class GlobalJsonSdkResolver
    {
        public const string LatestPatch = "latestPatch";
        public const string Patch = "patch";
        public const string LatestFeature = "latestFeature";
        public const string Feature = "feature";
        public const string LatestMinor = "latestMinor";
        public const string Minor = "minor";
        public const string LatestMajor = "latestMajor";
        public const string Major = "major";
        public const string Disable = "disable";

        private readonly ILogger<GlobalJsonSdkResolver> _logger;

        public GlobalJsonSdkResolver(ILogger<GlobalJsonSdkResolver> logger)
        {
            _logger = logger;
        }

        public string GetSatisfyingSdkVersion(GlobalJsonModel globalJson, IEnumerable<string> availableSdks)
        {
            /*
                From spec:
                If no global.json file is found, or global.json doesn't specify an SDK version nor an
                allowPrerelease value, the highest installed SDK version is used (equivalent to setting rollForward
                to latestMajor).
            */
            if (globalJson?.Sdk == null)
            {
                globalJson = new GlobalJsonModel();
                globalJson.Sdk = new Sdk
                {
                    Version = "0.0.000",
                    RollForward = LatestMajor,
                };

                _logger.LogDebug(
                    $"No 'sdk' provided in global.json. Choosing a version using the " +
                    $"default 'rollForward' policy: {globalJson.Sdk.RollForward}");
            }

            var sdkNodeInGlobalJson = globalJson.Sdk;

            // From spec: If no rollFoward value is set, it uses latestPatch as the default rollForward policy
            var rollForward = sdkNodeInGlobalJson.RollForward;
            if (string.IsNullOrEmpty(rollForward))
            {
                rollForward = LatestPatch;

                _logger.LogDebug(
                    $"No 'rollFoward' policy found in global.json. Choosing a version using the " +
                    $"default 'rollForward' policy: {rollForward}");
            }

            if (!SdkVersionInfo.TryParse(sdkNodeInGlobalJson.Version, out var sdkVersionInGlobalJson))
            {
                throw new InvalidUsageException($"Invalid version format '{sdkNodeInGlobalJson}' in global.json");
            }

            var availableSdkVersions = availableSdks.Select(sdk => SdkVersionInfo.Parse(sdk));

            // From spec:
            // If you don't set this value explicitly, the default value depends on whether you're running from
            // Visual Studio:
            // If you're not in Visual Studio, the default value is true.
            if (!string.IsNullOrEmpty(sdkNodeInGlobalJson.AllowPreRelease))
            {
                if (bool.TryParse(sdkNodeInGlobalJson.AllowPreRelease, out var allowPrerelease))
                {
                    if (!allowPrerelease)
                    {
                        availableSdkVersions = availableSdkVersions.Where(sdk => !sdk.IsPrerelease);
                    }
                }
                else
                {
                    throw new InvalidUsageException(
                        $"Invalid value {sdkNodeInGlobalJson.AllowPreRelease} for " +
                        $"'allowPrelease' in global.json. Allowed values are either 'true' or 'false'.");
                }
            }

            string resolvedVersion = null;
            switch (rollForward.ToLower())
            {
                case var policy when policy.EqualsIgnoreCase(Disable):
                    resolvedVersion = GetDisable(availableSdkVersions, sdkVersionInGlobalJson);
                    break;
                case var policy when policy.EqualsIgnoreCase(Patch):
                    resolvedVersion = GetPatch(availableSdkVersions, sdkVersionInGlobalJson);
                    break;
                case var policy when policy.EqualsIgnoreCase(Feature):
                    resolvedVersion = GetFeature(availableSdkVersions, sdkVersionInGlobalJson);
                    break;
                case var policy when policy.EqualsIgnoreCase(Minor):
                    resolvedVersion = GetMinor(availableSdkVersions, sdkVersionInGlobalJson);
                    break;
                case var policy when policy.EqualsIgnoreCase(Major):
                    resolvedVersion = GetMajor(availableSdkVersions, sdkVersionInGlobalJson);
                    break;
                case var policy when policy.EqualsIgnoreCase(LatestPatch):
                    resolvedVersion = GetLatestPatch(availableSdkVersions, sdkVersionInGlobalJson);
                    break;
                case var policy when policy.EqualsIgnoreCase(LatestFeature):
                    resolvedVersion = GetLatestFeature(availableSdkVersions, sdkVersionInGlobalJson);
                    break;
                case var policy when policy.EqualsIgnoreCase(LatestMinor):
                    resolvedVersion = GetLatestMinor(availableSdkVersions, sdkVersionInGlobalJson);
                    break;
                case var policy when policy.EqualsIgnoreCase(LatestMajor):
                    resolvedVersion = GetLatestMajor(availableSdkVersions, sdkVersionInGlobalJson);
                    break;
                default:
                    _logger.LogDebug(
                        "Value {invalidRollForwardPolicy} is invalid for 'rollFoward' policy.",
                        rollForward);
                    return null;
            }

            if (resolvedVersion == null)
            {
                _logger.LogDebug(
                    "Could not resolve a version using roll forward policy {rollForwardPolicy} and available sdk " +
                    "versions {availableSdkVersions}",
                    rollForward,
                    string.Join(", ", availableSdkVersions));
            }

            return resolvedVersion;
        }

        private string GetDisable(IEnumerable<SdkVersionInfo> availableSdks, SdkVersionInfo versionToResolve)
        {
            // From spec: Doesn't roll forward. Exact match required.
            if (availableSdks.Any(availableSdk => availableSdk.Equals(versionToResolve)))
            {
                return versionToResolve.RawString;
            }

            return null;
        }

        private string GetPatch(IEnumerable<SdkVersionInfo> availableSdks, SdkVersionInfo versionToResolve)
        {
            /*
                Uses the specified version.
                If not found, rolls forward to the latest patch level.
                If not found, fails.
                This value is the legacy behavior from the earlier versions of the SDK.
             */
            if (availableSdks.Any(availableSdk => availableSdk.Equals(versionToResolve)))
            {
                return versionToResolve.RawString;
            }
            else
            {
                return GetLatestPatch(availableSdks, versionToResolve);
            }
        }

        private string GetFeature(IEnumerable<SdkVersionInfo> availableSdks, SdkVersionInfo versionToResolve)
        {
            /*
                Uses the latest patch level for the specified major, minor, and feature band.
                If not found, rolls forward to the next higher feature band within the same major/minor and
                uses the latest patch level for that feature band.
                If not found, fails.
            */
            availableSdks = availableSdks.Where(availableSdk =>
            {
                return availableSdk.Major == versionToResolve.Major
                && availableSdk.Minor == versionToResolve.Minor;
            });

            var sameFeatureSdks = availableSdks.Where(availableSdk => availableSdk.Feature == versionToResolve.Feature);
            if (sameFeatureSdks.Any())
            {
                return GetLatestPatch(sameFeatureSdks, versionToResolve);
            }
            else
            {
                var higherFeatureSdks = availableSdks
                    .Where(availableSdk => availableSdk.Feature > versionToResolve.Feature);
                if (higherFeatureSdks.Any())
                {
                    var nextHigherFeatureSdk = higherFeatureSdks.OrderBy(sdk => sdk).First();
                    var nextHigherFeatureSdks = higherFeatureSdks.Where(sdk => sdk.Feature == nextHigherFeatureSdk.Feature);
                    return GetLatestPatch(nextHigherFeatureSdks, nextHigherFeatureSdk);
                }
            }

            return null;
        }

        private string GetMinor(IEnumerable<SdkVersionInfo> availableSdks, SdkVersionInfo versionToResolve)
        {
            /*
                Uses the latest patch level for the specified major, minor, and feature band.
                If not found, rolls forward to the next higher feature band within the same major/minor version
                and uses the latest patch level for that feature band.
                If not found, rolls forward to the next higher minor and feature band within the same major and
                uses the latest patch level for that feature band.
                If not found, fails.
            */
            var result = GetFeature(availableSdks, versionToResolve);
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }

            // If not found, rolls forward to the next higher minor and feature band within the same major and
            // uses the latest patch level for that feature band.
            availableSdks = availableSdks.Where(availableSdk =>
            {
                return availableSdk.Major == versionToResolve.Major;
            });

            if (availableSdks.Any())
            {
                var higherMinorSdks = availableSdks.Where(sdk => sdk.Minor > versionToResolve.Minor);
                if (higherMinorSdks.Any())
                {
                    var nextHighestMinorSdk = higherMinorSdks.OrderBy(sdk => sdk).First();
                    var nextHighestMinorSdks = higherMinorSdks.Where(sdk => sdk.Minor == nextHighestMinorSdk.Minor);
                    return GetFeature(nextHighestMinorSdks, nextHighestMinorSdk);
                }
            }

            return null;
        }

        private string GetMajor(IEnumerable<SdkVersionInfo> availableSdks, SdkVersionInfo versionToResolve)
        {
            /*
                Uses the latest patch level for the specified major, minor, and feature band.
                If not found, rolls forward to the next higher feature band within the same major/minor version
                and uses the latest patch level for that feature band.
                If not found, rolls forward to the next higher minor and feature band within the same major and
                uses the latest patch level for that feature band.
                If not found, rolls forward to the next higher major, minor, and feature band and uses the latest
                patch level for that feature band.
                If not found, fails.
             */
            var result = GetMinor(availableSdks, versionToResolve);
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }

            var higherMajorSdks = availableSdks.Where(sdk => sdk.Major > versionToResolve.Major);
            if (higherMajorSdks.Any())
            {
                var nextHighestMajorSdk = higherMajorSdks.OrderBy(sdk => sdk).First();
                var nextHighestMajorSdks = higherMajorSdks.Where(sdk => sdk.Major == nextHighestMajorSdk.Major);
                return GetMinor(nextHighestMajorSdks, nextHighestMajorSdk);
            }

            return null;
        }

        private string GetLatestPatch(IEnumerable<SdkVersionInfo> availableSdks, SdkVersionInfo versionToResolve)
        {
            /*
                Uses the latest installed patch level that matches the requested major, minor, and feature band
                with a patch level and that is greater or equal than the specified value.
                If not found, fails
             */
            availableSdks = availableSdks
                .Where(availableSdk =>
                {
                    return availableSdk.Major == versionToResolve.Major
                    && availableSdk.Minor == versionToResolve.Minor
                    && availableSdk.Feature == versionToResolve.Feature
                    && availableSdk.Patch >= versionToResolve.Patch;
                })
                .OrderByDescending(availableSdk => availableSdk);

            var satisfyingVersion = availableSdks.FirstOrDefault();
            if (satisfyingVersion != null)
            {
                return satisfyingVersion.RawString;
            }

            return null;
        }

        private string GetLatestFeature(IEnumerable<SdkVersionInfo> availableSdks, SdkVersionInfo versionToResolve)
        {
            /*
                Uses the highest installed feature band and patch level that matches the requested major and minor
                with a feature band that is greater or equal than the specified value.
                If not found, fails.
             */
            availableSdks = availableSdks
                .Where(availableSdk =>
                {
                    return availableSdk.Major == versionToResolve.Major
                    && availableSdk.Minor == versionToResolve.Minor
                    && availableSdk.Feature >= versionToResolve.Feature;
                })
                .OrderByDescending(availableSdk => availableSdk);

            var satisfyingVersion = availableSdks.FirstOrDefault();
            if (satisfyingVersion != null)
            {
                return satisfyingVersion.RawString;
            }

            return null;
        }

        private string GetLatestMinor(IEnumerable<SdkVersionInfo> availableSdks, SdkVersionInfo versionToResolve)
        {
            /*
                Uses the highest installed minor, feature band, and patch level that matches the requested major with
                a minor that is greater or equal than the specified value.
                If not found, fails.
             */
            availableSdks = availableSdks
                .Where(availableSdk =>
                {
                    return availableSdk.Major == versionToResolve.Major
                    && availableSdk.Minor >= versionToResolve.Minor;
                })
                .OrderByDescending(availableSdk => availableSdk);

            var satisfyingVersion = availableSdks.FirstOrDefault();
            if (satisfyingVersion != null)
            {
                return satisfyingVersion.RawString;
            }

            return null;
        }

        private string GetLatestMajor(IEnumerable<SdkVersionInfo> availableSdks, SdkVersionInfo versionToResolve)
        {
            /*
                Uses the highest installed .NET Core SDK with a major that is greater or equal than the specified value.
                If not found, fail.
             */
            availableSdks = availableSdks
                .Where(availableSdk =>
                {
                    return availableSdk.Major >= versionToResolve.Major;
                })
                .OrderByDescending(availableSdk => availableSdk);

            var satisfyingVersion = availableSdks.FirstOrDefault();
            if (satisfyingVersion != null)
            {
                return satisfyingVersion.RawString;
            }

            return null;
        }
    }
}
