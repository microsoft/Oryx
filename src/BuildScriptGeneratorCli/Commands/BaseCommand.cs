// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal abstract class BaseCommand
    {
        private IServiceProvider _serviceProvider = null;

        public int OnExecute(CommandLineApplication app, IConsole console)
        {
            console.CancelKeyPress += Console_CancelKeyPress;

            try
            {
                _serviceProvider = GetServiceProvider();
                if (!IsValidInput(_serviceProvider, console))
                {
                    return Constants.ExitFailure;
                }

                return Execute(_serviceProvider, console);
            }
            finally
            {
                DisposeServiceProvider();
            }
        }

        internal abstract int Execute(IServiceProvider serviceProvider, IConsole console);

        internal virtual void ConfigureBuildScriptGeneratorOptions(BuildScriptGeneratorOptions options)
        {
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
                    ConfigureBuildScriptGeneratorOptions(o);
                });
            return serviceProviderBuilder.Build();
        }

        private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            DisposeServiceProvider();
        }

        private void DisposeServiceProvider()
        {
            // In general it is a good practice to dispose services before this program is exiting, but there's
            // one more reason we would need to do this i.e that the File logger doesn't write to a file
            // immediately. This is because it queues up messages until a certain threshold is reached and then
            // flushes them.
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
