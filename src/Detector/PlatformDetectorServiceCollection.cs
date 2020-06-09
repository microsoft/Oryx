// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Oryx.Detector.DotNetCore;
using Microsoft.Oryx.Detector.Node;
using Microsoft.Oryx.Detector.Php;
using Microsoft.Oryx.Detector.Python;

namespace Microsoft.Oryx.Detector
{
    public static class PlatformDetectorServiceCollection
    {
        public static IServiceCollection AddPlatformDetectorServices(this IServiceCollection services)
        {
            services.AddSingleton<IDetector, DefaultPlatformDetector>();

            services.AddSingleton<NodePlatformDetector>();

            services.AddSingleton<DotNetCorePlatformDetector>();
            services.AddSingleton<DefaultProjectFileProvider>();
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IProjectFileProvider, ExplicitProjectFileProvider>());
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IProjectFileProvider, RootDirectoryProjectFileProvider>());
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IProjectFileProvider, ProbeAndFindProjectFileProvider>());


            services.AddSingleton<PhpPlatformDetector>();

            services.AddSingleton<PythonPlatformDetector>();

            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IPlatformDetector, NodePlatformDetector>());
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IPlatformDetector, DotNetCorePlatformDetector>());
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IPlatformDetector, PhpPlatformDetector>());
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IPlatformDetector, PythonPlatformDetector>());

            return services;
        }
    }
}