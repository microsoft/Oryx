using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdater
{
    public class GHRunnerCachedImgCheckBinder : BinderBase<GHRunnerCachedImgCheckProperty>
    {
        private Argument<string> sourceDir;

        public GHRunnerCachedImgCheckBinder(Argument<string> sourceDir)
        {
            this.sourceDir = sourceDir;
        }

        protected override GHRunnerCachedImgCheckProperty GetBoundValue(BindingContext bindingContext) =>
            new GHRunnerCachedImgCheckProperty
            {
                SourceDir = bindingContext.ParseResult.GetValueForArgument(this.sourceDir),
            };
    }
}
