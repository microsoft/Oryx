// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Ruby
{
    public static class RubyConstants
    {
        public const string PlatformName = "ruby";
        public const string GemToolName = "gem";
        public const string RubyFileNamePattern = "*.rb";
        public const string GemFileName = "Gemfile";
        public const string GemFileLockName = "Gemfile.lock";
        public const string ConfigRubyFileName = "config.ru";
        public const string RakeFileName = "Rakefile";
        public const string ConfigYmlFileName = "_config.yml";
        public const string DefaultAppLocationDirName = "_site";
        public const string RubyVersionEnvVarName = "RUBY_VERSION";
        public const string RubyLtsVersion = Common.RubyVersions.Ruby27Version;
        public const string InstalledRubyVersionsDir = "/opt/ruby/";
        public const string GemVersionCommand = "echo Using Gem version: && gem --version";
    }
}
