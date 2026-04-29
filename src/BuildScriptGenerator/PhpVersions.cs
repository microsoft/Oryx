// Current versions are read from images/constants.yml via ConstantsYamlReader.

using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator.Common
{
    public static class PhpVersions
    {
        public const string PhpRuntimeBaseTag = "20240430.1";

        public const string PhpFpmRuntimeBaseTag = "20240430.1";

        public const string ComposerSetupSha384 = "e21205b207c3ff031906575712edab6f13eb0b361f2085f1f1237b7126d785e826a450292b6cfd1d64d92e6563bbde02";

        public const string Composer19Version = "1.9.3";

        public const string Composer110Version = "1.10.19";

        public const string Composer20Version = "2.0.8";

        public const string Composer22Version = "2.2.21";

        public const string Composer23Version = "2.3.10";

        public const string Composer24Version = "2.4.4";

        public const string Composer25Version = "2.5.8";

        public const string Composer26Version = "2.6.2";

        public const string Composer27Version = "2.7.7";

        public const string Composer28Version = "2.8.8";

        public const string Php80Version = "8.0.30";

        public const string Php80Keys = "1729F83938DA44E27BA0F4D3DBDB397470D12172 BFDDD28642824F8118EF77909B67A5C12229118F";

        public const string Php80TarSha256 = "216ab305737a5d392107112d618a755dc5df42058226f1670e9db90e77d777d9";

        public const string Php74Version = "7.4.33";

        public const string Php74Keys = "42670A7FE4D0441C8E4632349E4FDC074A4EF02D 5A52880781F755608BF815FC910DEB46F53EA312";

        public const string Php74TarSha256 = "924846abf93bc613815c55dd3f5809377813ac62a9ec4eb3778675b82a27b927";

        public const string Php73Version = "7.3.27";

        public const string Php73Keys = "CBAF69F173A0FEA4B537F470D66C9593118BCCB6 F38252826ACD957EF380D39F2F7956BC5DA04B5D";

        public const string Php73TarSha256 = "65f616e2d5b6faacedf62830fa047951b0136d5da34ae59e6744cbaf5dca148d";

        public const string Php72Version = "7.2.34";

        public const string Php72Keys = "1729F83938DA44E27BA0F4D3DBDB397470D12172 B1B44D8F021E4E2D6021E995DC9FF8D3EE5AF27F";

        public const string Php72TarSha256 = "409e11bc6a2c18707dfc44bc61c820ddfd81e17481470f3405ee7822d8379903";

        public const string Php70Version = "7.0.33";

        public const string Php70Keys = "1A4E8B7277C42E53DBA9C7B9BCAA30EA9C0D5763 6E4F6AB321FDC07F2C332E3AC2BF0BC433CFC8B3";

        public const string Php70TarSha256 = "ab8c5be6e32b1f8d032909dedaaaa4bbb1a209e519abb01a52ce3914f9a13d96";

        public const string Php56Version = "5.6.40";

        public const string Php56Keys = "0BD78B5F97500D450838F95DFE857D9A90D90EC1 6E4F6AB321FDC07F2C332E3AC2BF0BC433CFC8B3";

        public const string Php56TarSha256 = "1369a51eee3995d7fbd1c5342e5cc917760e276d561595b6052b21ace2656d1c";

        public static readonly List<string> RuntimeVersions = new List<string> { "7.4-debian-bullseye", "7.4-debian-buster", "8.0-debian-bullseye", "8.0-debian-buster", "8.1-debian-bullseye", "8.1-debian-buster", "8.2-debian-bullseye", "8.2-debian-buster", "8.3-debian-bullseye", "8.3-debian-buster", "8.3-debian-bookworm" };

        public static readonly List<string> FpmRuntimeVersions = new List<string> { "7.4-fpm-debian-bullseye", "8.0-fpm-debian-bullseye", "8.1-fpm-debian-bullseye", "8.2-fpm-debian-bullseye", "8.3-fpm-debian-bullseye", "8.3-fpm-debian-bookworm", "8.4-fpm-debian-bullseye", "8.4-fpm-debian-bookworm", "8.5-fpm-ubuntu-noble" };

        public static readonly Dictionary<string, string[]> ComposerVersionsPerDebianFlavor = new Dictionary<string, string[]>
        {
            { "bullseye", new[] { "1.9.2", "1.9.3", "1.10.0", "1.10.1", "1.10.2", "1.10.4", "1.10.5", "1.10.6", "1.10.7", "1.10.8", "1.10.9", "1.10.10", "1.10.11", "1.10.12", "1.10.13", "1.10.14", "1.10.15", "1.10.16", "1.10.17", "1.10.18", "1.10.19", "2.0.0", "2.0.1", "2.0.2", "2.0.3", "2.0.4", "2.0.5", "2.0.6", "2.0.7", "2.0.8", "2.2.9", "2.2.21", "2.3.4", "2.3.10", "2.4.4", "2.5.8", "2.6.2", "2.7.7", "2.8.2", "2.8.4", "2.8.6", "2.8.8" } },
            { "buster", new[] { "1.9.2", "1.9.3", "1.10.0", "1.10.1", "1.10.2", "1.10.4", "1.10.5", "1.10.6", "1.10.7", "1.10.8", "1.10.9", "1.10.10", "1.10.11", "1.10.12", "1.10.13", "1.10.14", "1.10.15", "1.10.16", "1.10.17", "1.10.18", "1.10.19", "2.0.0", "2.0.1", "2.0.2", "2.0.3", "2.0.4", "2.0.5", "2.0.6", "2.0.7", "2.0.8", "2.2.9", "2.2.21", "2.3.4", "2.3.10", "2.4.4", "2.5.8", "2.6.2" } },
            { "bookworm", new[] { "2.0.8", "2.6.2", "2.7.7", "2.8.2", "2.8.4", "2.8.6", "2.8.8" } },
            { "noble", new[] { "2.6.2", "2.7.7", "2.8.2", "2.8.4", "2.8.6", "2.8.8" } },
        };

        public static string ComposerDefaultVersion => ConstantsYamlReader.Get("phpComposerVersion");

        public static string Php85Version => ConstantsYamlReader.Get("php85Version");

        public static string Php85Keys => ConstantsYamlReader.TryGet("php85_GPG_keys");

        public static string Php85TarSha256 => ConstantsYamlReader.Get("php85Version_SHA");

        public static string Php84Version => ConstantsYamlReader.Get("php84Version");

        public static string Php84Keys => ConstantsYamlReader.Get("php84_GPG_keys");

        public static string Php84TarSha256 => ConstantsYamlReader.Get("php84Version_SHA");

        public static string Php83Version => ConstantsYamlReader.Get("php83Version");

        public static string Php83Keys => ConstantsYamlReader.Get("php83_GPG_keys");

        public static string Php83TarSha256 => ConstantsYamlReader.Get("php83Version_SHA");

        public static string Php82Version => ConstantsYamlReader.Get("php82Version");

        public static string Php82Keys => ConstantsYamlReader.Get("php82_GPG_keys");

        public static string Php82TarSha256 => ConstantsYamlReader.Get("php82Version_SHA");

        public static string Php81Version => ConstantsYamlReader.Get("php81Version");

        public static string Php81Keys => ConstantsYamlReader.Get("php81_GPG_keys");

        public static string Php81TarSha256 => ConstantsYamlReader.Get("php81Version_SHA");
    }
}