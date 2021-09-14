// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Oryx.BuildServer.Interfaces;
using System;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildServer.Controllers
{
    public class ProcessCallHandler : IProcessCallHandler
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public ProcessCallHandler (IServiceScopeFactory scopeFactory)
        {
            _serviceScopeFactory = scopeFactory;
        }

        public void Execute(Func<IBuildServerAction, Task> buildServerTask)
        {
            // Fire off the task, but don't await the result
            Task.Run(async () =>
            {
                // Exceptions must be caught
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var action = scope.ServiceProvider.GetRequiredService<IBuildServerAction>();
                    await buildServerTask(action);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
        }
    }
}
