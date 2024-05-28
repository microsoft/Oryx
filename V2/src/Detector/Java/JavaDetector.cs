// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Linq;
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.Detector.Java
{
    /// <summary>
    /// An implementation of <see cref="IPlatformDetector"/> which detects NodeJS applications.
    /// </summary>
    public class JavaDetector : IJavaPlatformDetector
    {
        private readonly ILogger<JavaDetector> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaDetector"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger{JavaDetector}"/>.</param>
        public JavaDetector(ILogger<JavaDetector> logger)
        {
            this.logger = logger;
        }

        /// <inheritdoc/>
        public PlatformDetectorResult Detect(DetectorContext context)
        {
            var sourceRepo = context.SourceRepo;
            var hasJavaRelatedFileExtensions = false;
            foreach (var fileExtensionName in JavaConstants.JavaFileExtensionNames)
            {
                var files = sourceRepo.EnumerateFiles($"*.{fileExtensionName}", searchSubDirectories: true);
                if (files.Any())
                {
                    this.logger.LogDebug($"Found files with extension '{fileExtensionName}' in the repo");
                    hasJavaRelatedFileExtensions = true;
                    break;
                }
            }

            if (!hasJavaRelatedFileExtensions)
            {
                this.logger.LogDebug(
                    $"Could not find any files with the following extensions in the repo: " +
                    $"{string.Join(", ", JavaConstants.JavaFileExtensionNames)}");
                return null;
            }

            var result = new JavaPlatformDetectorResult();
            result.Platform = JavaConstants.PlatformName;
            if (sourceRepo.FileExists(MavenConstants.PomXmlFileName))
            {
                result.UsesMaven = true;
            }

            if (sourceRepo.FileExists(MavenConstants.MavenWrapperShellFileName)
                || sourceRepo.FileExists(MavenConstants.MavenWrapperCmdFileName))
            {
                result.UsesMavenWrapperTool = true;
            }

            return result;
        }
    }
}
