// Current versions are read from images/constants.yml via ConstantsYamlReader.

using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator.Common
{
    public static class PythonVersions
    {
        public const string PythonRuntimeBaseTag = "20240430.1";

        public const string PipVersion = "21.2.4";

        public const string Python27Version = "2.7.18";

        public const string Python36Version = "3.6.15";

        public const string Python37Version = "3.7.15";

        public const string Python38Version = "3.8.20";

        public static readonly List<string> RuntimeVersions = new List<string> { "3.7-debian-bullseye", "3.7-debian-buster", "3.8-debian-bookworm", "3.8-debian-bullseye", "3.8-debian-buster", "3.9-debian-bookworm", "3.9-debian-bullseye", "3.9-debian-buster", "3.10-debian-bookworm", "3.10-debian-bullseye", "3.10-debian-buster", "3.11-debian-bookworm", "3.11-debian-bullseye", "3.12-debian-bookworm", "3.12-debian-bullseye", "3.13-debian-bullseye", "3.13-debian-bookworm", "dynamic-debian-buster" };

        public static string Python39Version => ConstantsYamlReader.Get("python39Version");

        public static string Python310Version => ConstantsYamlReader.Get("python310Version");

        public static string Python311Version => ConstantsYamlReader.Get("python311Version");

        public static string Python312Version => ConstantsYamlReader.Get("python312Version");

        public static string Python313Version => ConstantsYamlReader.Get("python313Version");

        public static string Python314Version => ConstantsYamlReader.TryGet("python314Version");
    }
}