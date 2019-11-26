// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Commands
{
    [Command(
        Name,
        Description = "Gets the maximum satisfying version for a given range and list of versions",
        ThrowOnUnexpectedArgument = false,
        AllowArgumentSeparator = true)]
    internal class SemVerResolveCommand : CommandBase
    {
        public const string Name = "resolveVersion";

        [Argument(0, Description = "Version range.")]
        public string Range { get; set; }

        [Argument(1, Description = "List of supported versions, comma separated")]
        public string SupportedVersions { get; set; }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            var supportedVersions = SupportedVersions.Split(",").Select(version => version.Trim());

            // We ignore text like 'lts' etc and let the underlying scripts to handle them.
            var result = Range;
            if (TryGetRange(Range, out var range))
            {
                result = range.MaxSatisfying(supportedVersions);
                if (string.IsNullOrEmpty(result))
                {
                    return 1;
                }
            }

            console.Write(result);
            return 0;
        }

        private bool TryGetRange(string suppliedRange, out SemVer.Range range)
        {
            range = null;

            try
            {
                range = new SemVer.Range(suppliedRange);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
