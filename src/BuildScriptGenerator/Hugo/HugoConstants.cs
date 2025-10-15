// This file was auto-generated from 'constants.yaml'. Changes may be overridden.

using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator.Hugo
{
    public static class HugoConstants
    {
        public const string Version = "0.151.0";
        public const string PlatformName = "hugo";
        public const string InstalledHugoVersionsDir = "/opt/hugo";
        public const string InstallationUrlFormat = "https://github.com/gohugoio/hugo/releases/download/v#VERSION#/#TAR_FILE#";
        public const string TarFileNameFormat = "hugo_extended_#VERSION#_Linux-64bit.tar.gz";
        public const string ConfigFolderName = "config";
        public static readonly List<string> TomlFileNames = new List<string> { "config.toml", "hugo.toml" };
        public static readonly List<string> YamlFileNames = new List<string> { "config.yaml", "hugo.yaml" };
        public static readonly List<string> YmlFileNames = new List<string> { "config.yml", "hugo.yml" };
        public static readonly List<string> JsonFileNames = new List<string> { "config.json", "hugo.json" };
    }
}