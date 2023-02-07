using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Commands
{
    public class PrepareEnvironmentCommandBinder : CommandBaseBinder<PrepareEnvironmentCommandProperty>
    {
        private Option<string> sourceDirOption;
        private Option<bool> skipDetectionOption;
        private Option<string> platformsAndVersionsOption;
        private Option<string> platformsAndVersionsFileOption;
        private Option<string> logFileOption;
        private Option<bool> debugOption;

        public PrepareEnvironmentCommandBinder(
            Option<string> sourceDirOption,
            Option<bool> skipDetectionOption,
            Option<string> platformsAndVersionsOption,
            Option<string> platformsAndVersionsFileOption,
            Option<string> logFileOption,
            Option<bool> debugOption)
            : base(logFileOption, debugOption)
        {
            this.sourceDirOption = sourceDirOption;
            this.skipDetectionOption = skipDetectionOption;
            this.platformsAndVersionsOption = platformsAndVersionsOption;
            this.platformsAndVersionsFileOption = platformsAndVersionsFileOption;
        }

        protected override PrepareEnvironmentCommandProperty GetBoundValue(BindingContext bindingContext) =>
            new PrepareEnvironmentCommandProperty
            {
                SourceDir = bindingContext.ParseResult.GetValueForOption(this.sourceDirOption),
                SkipDetection = bindingContext.ParseResult.GetValueForOption(this.skipDetectionOption),
                PlatformsAndVersions = bindingContext.ParseResult.GetValueForOption(this.platformsAndVersionsOption),
                PlatformsAndVersionsFile = bindingContext.ParseResult.GetValueForOption(this.platformsAndVersionsFileOption),
                LogFilePath = bindingContext.ParseResult.GetValueForOption(this.logFileOption),
                DebugMode = bindingContext.ParseResult.GetValueForOption(this.debugOption),
            };
    }
}
