// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Detector.DotNetCore;
using Microsoft.Oryx.Detector.Hugo;
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
            services.AddSingleton<IConfigureOptions<DetectorOptions>, DetectorOptionsSetup>();

            // .NET Core
            services.AddSingleton<DotNetCoreDetector>();
            services.AddSingleton<DefaultProjectFileProvider>();
            // Note that the order of these project file providers is important. For example, if a user explicitly
            // specifies a project file using either the 'PROJECT' environment or the 'project' build property, we want
            // to use that. In that case we want the ExplicitProjectFileProvider to return the project file and not
            // probe for files.
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IPlatformDetector, DotNetCoreDetector>());
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IProjectFileProvider, ExplicitProjectFileProvider>());
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IProjectFileProvider, RootDirectoryProjectFileProvider>());
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IProjectFileProvider, ProbeAndFindProjectFileProvider>());

            // Node
            services.AddSingleton<NodeDetector>();
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IPlatformDetector, NodeDetector>());

            // Python
            services.AddSingleton<PythonDetector>();
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IPlatformDetector, PythonDetector>());

            // PHP
            services.AddSingleton<PhpDetector>();
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IPlatformDetector, PhpDetector>());

            // Hugo
            services.AddSingleton<HugoDetector>();
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IPlatformDetector, HugoDetector>());
            services.AddSingleton<IConfigureOptions<HugoDetectorOptions>, HugoDetectorOptionsSetup>();

            return services;
        }
    }
}