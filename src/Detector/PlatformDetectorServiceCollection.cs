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
            services.AddSingleton<DefaultProjectFileProvider>();
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IProjectFileProvider, ExplicitProjectFileProvider>());
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IProjectFileProvider, RootDirectoryProjectFileProvider>());
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IProjectFileProvider, ProbeAndFindProjectFileProvider>());

            services.AddSingleton<NodeDetector>();
            services.AddSingleton<DotNetCoreDetector>();
            services.AddSingleton<PythonDetector>();
            services.AddSingleton<PhpDetector>();

            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IPlatformDetector, NodeDetector>());
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IPlatformDetector, DotNetCoreDetector>());
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IPlatformDetector, PythonDetector>());
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IPlatformDetector, PhpDetector>());

            services.AddSingleton<IDetector, DefaultPlatformDetector>();

            return services;
        }
    }
}