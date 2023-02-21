using System.Collections.Generic;

namespace Oryx.Microsoft.Automation.Python
{
    public class Constants
    {
        public const string ConstantsYaml = "constants.yaml";
        public const string PythonName = "python";
        public const string VersionsToBuildTxt = "versionsToBuild.txt";
        public const string YamlPythonKey = "python-versions";
        public static readonly Dictionary<string, string> VersionGpgKeys = new Dictionary<string, string>()
        {
            { "3.10", "A035C8C19219BA821ECEA86B64E628F8D684696D" },
            { "3.11", "A035C8C19219BA821ECEA86B64E628F8D684696D" },
        };
    }
}
