using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Commands
{
    public abstract class BuildCommandBaseBinder<T> : CommandBaseBinder<T>
        where T : BuildCommandBaseProperty
    {
#pragma warning disable SA1401 // Fields should be private
        protected Argument<string> sourceDir;
        protected Option<string> platform;
        protected Option<string> platformVersion;
        protected Option<bool> package;
        protected Option<string> osRequirements;
        protected Option<string> appType;
        protected Option<string> buildCommandFile;
        protected Option<bool> compressDestinationDir;
        protected Option<string[]> property;
        protected Option<string> dynamicInstallRootDir;
#pragma warning restore SA1401 // Fields should be private

        public BuildCommandBaseBinder(
            Argument<string> sourceDir,
            Option<string> platform,
            Option<string> platformVersion,
            Option<bool> package,
            Option<string> osRequirements,
            Option<string> appType,
            Option<string> buildCommandFile,
            Option<bool> compressDestinationDir,
            Option<string[]> property,
            Option<string> dynamicInstallRootDir,
            Option<string> logPath,
            Option<bool> debugMod)
            : base(logPath, debugMod)
        {
            this.sourceDir = sourceDir;
            this.platform = platform;
            this.platformVersion = platformVersion;
            this.package = package;
            this.osRequirements = osRequirements;
            this.appType = appType;
            this.buildCommandFile = buildCommandFile;
            this.compressDestinationDir = compressDestinationDir;
            this.property = property;
            this.dynamicInstallRootDir = dynamicInstallRootDir;
        }
    }
}
