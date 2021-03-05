// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.Detector.Node
{
    /// <summary>
    /// Represents a <see cref="PlatformDetectorResult"/> returned by the <see cref="NodeDetector"/> and
    /// contains additional information related to the detected applications.
    /// </summary>
    public class NodePlatformDetectorResult : PlatformDetectorResult
    {
        /// <summary>
        /// Gets or sets a list of detected framework information of an application.
        /// </summary>
        public IEnumerable<FrameworkInfo> Frameworks { get; set; }

        public bool HasLernaJsonFile { get; set; }

        public string LernaNpmClient { get; set; }

        public bool HasLageConfigJSFile { get; set; }

        public bool HasYarnrcYmlFile { get; set; }

        public bool IsYarnLockFileValidYamlFormat { get; set; }
    }
}
