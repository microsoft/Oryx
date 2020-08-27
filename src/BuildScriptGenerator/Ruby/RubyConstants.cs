// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Ruby
{
    internal static class RubyConstants
    {
        internal const string PlatformName = "ruby";
        internal const string GemToolName = "gem";
        internal const string RubyFileNamePattern = "*.rb";
        internal const string GemFileName = "Gemfile";
        internal const string GemFileLockName = "Gemfile.lock";
        internal const string ConfigRubyFileName = "config.ru";
        internal const string RakeFileName = "Rakefile";
        internal const string RubyVersionEnvVarName = "RUBY_VERSION";
        internal const string RubyLtsVersion = Common.RubyVersions.Ruby27Version;
        internal const string InstalledRubyVersionsDir = "/opt/ruby/";
        internal const string GemVersionCommand = "echo Using Gem version: && gem --version";
    }
}
