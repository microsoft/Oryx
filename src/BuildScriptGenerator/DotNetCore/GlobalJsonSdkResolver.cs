// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Newtonsoft.Json;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    public class GlobalJsonSdkResolver
    {
        private readonly ILogger<GlobalJsonSdkResolver> logger;

        public GlobalJsonSdkResolver(ILogger<GlobalJsonSdkResolver> logger)
        {
            this.logger = logger;
        }

        public string GetSatisfyingSdkVersion(
            ISourceRepo sourceRepo,
            string runtimeVersion,
            IEnumerable<string> availableSdks)
        {
            string sdkVersion;
            if (sourceRepo.FileExists(DotNetCoreConstants.GlobalJsonFileName))
            {
                var globalJsonContent = sourceRepo.ReadFile(
                    Path.Combine(sourceRepo.RootPath, DotNetCoreConstants.GlobalJsonFileName));

                this.logger.LogDebug(
                    "Detected presence of global.json file with content {globalJsonContent}",
                    globalJsonContent);

                var globalJsonModel = JsonConvert.DeserializeObject<GlobalJsonModel>(globalJsonContent);
                sdkVersion = this.GetSatisfyingSdkVersion(globalJsonModel, availableSdks);

                this.logger.LogDebug(
                    "Resolved sdk version to {resolvedSdkVersion} based on global.json file and available sdk versions",
                    sdkVersion);
            }
            else
            {
                // As per global.json spec, if a global.json file is not present, then roll forward policy is
                // considered as 'latestMajor'. This can cause end users apps to fail since in this case even prelreease
                // versions are considered. So here we minimize the impact by relying on the runtime version instead.
                // We choose only the 'major' and 'minor' part of the runtime version.
                // For example, 2.1.14 of runtime will result in a latest minor sdk in '1', for example
                // 2.1.202 or 2.1.400
                var version = new SemanticVersioning.Version(runtimeVersion);
                var globalJsonModel = new GlobalJsonModel
                {
                    Sdk = new SdkModel
                    {
                        Version = $"{version.Major}.{version.Minor}.100",

                        // Get latest feature and patch of the version
                        RollForward = RollForwardPolicy.LatestFeature,
                        AllowPreRelease = true,
                    },
                };

                this.logger.LogDebug(
                    "global.json file was not find in the repo, so choosing an sdk version which satisfies the " +
                    "version {defaultSdkVersion}, roll forward policy of {defaultRollForwardPolicy} and " +
                    "allowPrerelease value of {defaultAllowPrerelease}.",
                    globalJsonModel.Sdk.Version,
                    globalJsonModel.Sdk.RollForward,
                    globalJsonModel.Sdk.AllowPreRelease);

                sdkVersion = this.GetSatisfyingSdkVersion(globalJsonModel, availableSdks);
            }

            return sdkVersion;
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
                globalJson.Sdk = new SdkModel
                {
                    Version = "0.0.000",
                    RollForward = RollForwardPolicy.LatestMajor,
                };

                this.logger.LogDebug(
                    $"No 'sdk' provided in global.json. Choosing a version using the " +
                    $"default 'rollForward' policy: {globalJson.Sdk.RollForward}");
            }

            var sdkNodeInGlobalJson = globalJson.Sdk;

            if (!SdkVersionInfo.TryParse(sdkNodeInGlobalJson.Version, out var sdkVersionInGlobalJson))
            {
                throw new InvalidUsageException($"Invalid version format '{sdkNodeInGlobalJson}' in global.json");
            }

            var parsedSdkVersions = new List<SdkVersionInfo>();
            var unparsedSdkVersions = new List<string>();
            foreach (var sdkVersion in availableSdks)
            {
                if (SdkVersionInfo.TryParse(sdkVersion, out var parsedSdkVersion))
                {
                    parsedSdkVersions.Add(parsedSdkVersion);
                }
                else
                {
                    unparsedSdkVersions.Add(sdkVersion);
                }
            }

            if (unparsedSdkVersions.Count > 0)
            {
                this.logger.LogDebug(
                    "Unable to parse sdk versions: {unparsedSdkVersions}",
                    string.Join(", ", unparsedSdkVersions));
            }

            var availableSdkVersions = parsedSdkVersions.AsEnumerable();

            if (!sdkNodeInGlobalJson.AllowPreRelease)
            {
                availableSdkVersions = availableSdkVersions.Where(sdk => !sdk.IsPrerelease);
            }

            string resolvedVersion = null;
            switch (sdkNodeInGlobalJson.RollForward)
            {
                case RollForwardPolicy.Disable:
                    resolvedVersion = GetDisable(availableSdkVersions, sdkVersionInGlobalJson);
                    break;
                case RollForwardPolicy.Patch:
                    resolvedVersion = GetPatch(availableSdkVersions, sdkVersionInGlobalJson);
                    break;
                case RollForwardPolicy.Feature:
                    resolvedVersion = GetFeature(availableSdkVersions, sdkVersionInGlobalJson);
                    break;
                case RollForwardPolicy.Minor:
                    resolvedVersion = GetMinor(availableSdkVersions, sdkVersionInGlobalJson);
                    break;
                case RollForwardPolicy.Major:
                    resolvedVersion = GetMajor(availableSdkVersions, sdkVersionInGlobalJson);
                    break;
                case RollForwardPolicy.LatestPatch:
                    resolvedVersion = GetLatestPatch(availableSdkVersions, sdkVersionInGlobalJson);
                    break;
                case RollForwardPolicy.LatestFeature:
                    resolvedVersion = GetLatestFeature(availableSdkVersions, sdkVersionInGlobalJson);
                    break;
                case RollForwardPolicy.LatestMinor:
                    resolvedVersion = GetLatestMinor(availableSdkVersions, sdkVersionInGlobalJson);
                    break;
                case RollForwardPolicy.LatestMajor:
                    resolvedVersion = GetLatestMajor(availableSdkVersions, sdkVersionInGlobalJson);
                    break;
                default:
                    this.logger.LogDebug(
                        "Value {invalidRollForwardPolicy} is invalid for 'rollFoward' policy.",
                        sdkNodeInGlobalJson.RollForward.ToString());
                    return null;
            }

            if (resolvedVersion == null)
            {
                this.logger.LogDebug(
                    "Could not resolve a version using roll forward policy {rollForwardPolicy} and available sdk " +
                    "versions {availableSdkVersions}",
                    sdkNodeInGlobalJson.RollForward.ToString(),
                    string.Join(", ", availableSdks));
            }

            return resolvedVersion;
        }

        private static string GetDisable(IEnumerable<SdkVersionInfo> availableSdks, SdkVersionInfo versionToResolve)
        {
            // From spec: Doesn't roll forward. Exact match required.
            if (availableSdks.Any(availableSdk => availableSdk.Equals(versionToResolve)))
            {
                return versionToResolve.RawString;
            }

            return null;
        }

        private static string GetPatch(IEnumerable<SdkVersionInfo> availableSdks, SdkVersionInfo versionToResolve)
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

        private static string GetFeature(IEnumerable<SdkVersionInfo> availableSdks, SdkVersionInfo versionToResolve)
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

        private static string GetMinor(IEnumerable<SdkVersionInfo> availableSdks, SdkVersionInfo versionToResolve)
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

        private static string GetMajor(IEnumerable<SdkVersionInfo> availableSdks, SdkVersionInfo versionToResolve)
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

        private static string GetLatestPatch(IEnumerable<SdkVersionInfo> availableSdks, SdkVersionInfo versionToResolve)
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

        private static string GetLatestFeature(IEnumerable<SdkVersionInfo> availableSdks, SdkVersionInfo versionToResolve)
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

        private static string GetLatestMinor(IEnumerable<SdkVersionInfo> availableSdks, SdkVersionInfo versionToResolve)
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

        private static string GetLatestMajor(IEnumerable<SdkVersionInfo> availableSdks, SdkVersionInfo versionToResolve)
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
