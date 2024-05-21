// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Oryx.Detector.Python;

namespace Microsoft.Oryx.Detector
{
    internal static class PythonServiceCollectionExtensions
    {
        public static IServiceCollection AddPythonServices(this IServiceCollection services)
        {
            services.AddSingleton<PythonDetector>();

            // Factory to make sure same detector instance is returned when same implementation type is resolved via
            // multiple inteface types.
            Func<IServiceProvider, PythonDetector> factory = (sp) => sp.GetRequiredService<PythonDetector>();
            services.AddSingleton<IPythonPlatformDetector, PythonDetector>(factory);
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IPlatformDetector, PythonDetector>(factory));
            return services;
        }
    }
}
