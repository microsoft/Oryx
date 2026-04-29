// Current versions are read from images/constants.yml via ConstantsYamlReader.

using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    public static class NodeVersions
    {
        public const string NodeRuntimeBaseTag = "20240429.6";

        public const string Node6Version = "6.17.1";

        public const string Node8Version = "8.17.0";

        public const string Node10Version = "10.23.0";

        public const string Node12Version = "12.22.12";

        public const string Node14Version = "14.21.3";

        public const string Node16Version = "16.20.2";

        public static readonly List<string> RuntimeVersions = new List<string> { "14-debian-bullseye", "14-debian-buster", "16-debian-bullseye", "16-debian-buster", "18-debian-bullseye", "20-debian-bullseye", "20-debian-bookworm", "22-debian-bullseye", "22-debian-bookworm", "dynamic-debian-buster" };

        public static string YarnVersion => ConstantsYamlReader.Get("YARN_VERSION");

        public static string YarnMinorVersion => ConstantsYamlReader.Get("YARN_MINOR_VERSION");

        public static string YarnMajorVersion => ConstantsYamlReader.Get("YARN_MAJOR_VERSION");

        public static string Node18Version => ConstantsYamlReader.Get("node18Version");

        public static string Node20Version => ConstantsYamlReader.Get("node20Version");

        public static string Node22Version => ConstantsYamlReader.Get("node22Version");

        public static string Node24Version => ConstantsYamlReader.Get("node24Version");

        public static string NodeAppInsightsSdkVersion => ConstantsYamlReader.Get("NODE_APP_INSIGHTS_SDK_VERSION");

        public static string Pm2Version => ConstantsYamlReader.Get("PM2_VERSION");

        public static string NpmVersion => ConstantsYamlReader.Get("NPM_VERSION");
    }
}