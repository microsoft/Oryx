// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Detector.DotNetCore;

namespace Microsoft.Oryx.Detector.Tests.DotNetCore
{
    internal static class ProjectFileProviderHelper
    {
        public static DefaultProjectFileProvider GetProjectFileProvider(DetectorOptions options = null)
        {
            if (options == null)
            {
                options = new DetectorOptions();
            }

            var providers = new IProjectFileProvider[]
            {
                new ExplicitProjectFileProvider(
                    Options.Create(options),
                    NullLogger<ExplicitProjectFileProvider>.Instance),
                new RootDirectoryProjectFileProvider(NullLogger<RootDirectoryProjectFileProvider>.Instance),
                new ProbeAndFindProjectFileProvider(NullLogger<ProbeAndFindProjectFileProvider>.Instance),
            };

            return new DefaultProjectFileProvider(providers);
        }
    }
}
