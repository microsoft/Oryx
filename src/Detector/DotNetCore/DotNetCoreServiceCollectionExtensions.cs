// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Oryx.Detector.DotNetCore;

namespace Microsoft.Oryx.Detector
{
    internal static class DotNetCoreServiceCollectionExtensions
    {
        public static IServiceCollection AddDotNetCoreServices(this IServiceCollection services)
        {
            services.AddSingleton<DotNetCoreDetector>();

            // Factory to make sure same detector instance is returned when same implementation type is resolved via
            // multiple inteface types.
            Func<IServiceProvider, DotNetCoreDetector> factory = (sp) => sp.GetRequiredService<DotNetCoreDetector>();
            services.AddSingleton<IDotNetCorePlatformDetector, DotNetCoreDetector>(factory);
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IPlatformDetector, DotNetCoreDetector>(factory));

            // Note that the order of these project file providers is important. For example, if a user explicitly
            // specifies a project file using either the 'PROJECT' environment or the 'project' build property, we want
            // to use that. In that case we want the ExplicitProjectFileProvider to return the project file and not
            // probe for files.
            services.AddSingleton<DefaultProjectFileProvider>();
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IProjectFileProvider, ExplicitProjectFileProvider>());
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IProjectFileProvider, RootDirectoryProjectFileProvider>());
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IProjectFileProvider, ProbeAndFindProjectFileProvider>());
            return services;
        }
    }
}
