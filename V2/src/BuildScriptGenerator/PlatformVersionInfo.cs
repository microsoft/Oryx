using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class PlatformVersionInfo
    {
        private PlatformVersionInfo()
        {
        }

        public IEnumerable<string> SupportedVersions { get; private set; }

        public string DefaultVersion { get; private set; }

        public PlatformVersionSourceType PlatformVersionSourceType { get; private set; }

        public static PlatformVersionInfo CreateOnDiskVersionInfo(
            IEnumerable<string> supportedVersions,
            string defaultVersion)
        {
            return new PlatformVersionInfo
            {
                SupportedVersions = supportedVersions,
                DefaultVersion = defaultVersion,
                PlatformVersionSourceType = PlatformVersionSourceType.OnDisk,
            };
        }

        public static PlatformVersionInfo CreateAvailableOnWebVersionInfo(
            IEnumerable<string> supportedVersions,
            string defaultVersion)
        {
            return new PlatformVersionInfo
            {
                SupportedVersions = supportedVersions,
                DefaultVersion = defaultVersion,
                PlatformVersionSourceType = PlatformVersionSourceType.AvailableOnWeb,
            };
        }
    }
}
