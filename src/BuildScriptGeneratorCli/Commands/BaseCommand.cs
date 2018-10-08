// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal abstract class BaseCommand
    {
        public int OnExecute(CommandLineApplication app, IConsole console)
        {
            if (ShowHelp())
            {
                app.ShowHelp();
                return 1;
            }

            IServiceProvider serviceProvider = null;
            try
            {
                serviceProvider = GetServiceProvider();
                if (!IsValidInput(serviceProvider, console))
                {
                    return 1;
                }

                return Execute(serviceProvider, console);
            }
            finally
            {
                // In general it is a good practice to dispose services before this program is
                // exiting, but there's one more reason we would need to do this i.e that the Console
                // logger doesn't write to the console immediately. This is because it runs on a separate
                // thread where it queues up messages and writes the console when the queue reaches a certain
                // threshold.
                if (serviceProvider is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        internal abstract int Execute(IServiceProvider serviceProvider, IConsole console);

        internal virtual void ConfigureBuildScriptGeneratorOptoins(BuildScriptGeneratorOptions options)
        {
        }

        internal virtual bool ShowHelp()
        {
            return false;
        }

        internal virtual bool IsValidInput(IServiceProvider serviceProvider, IConsole console)
        {
            return true;
        }

        internal virtual IServiceProvider GetServiceProvider()
        {
            var serviceProviderBuilder = new ServiceProviderBuilder()
                .ConfigureScriptGenerationOptions(o =>
                {
                    ConfigureBuildScriptGeneratorOptoins(o);
                });
            return serviceProviderBuilder.Build();
        }
    }
}
