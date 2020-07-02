// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Oryx.Detector.Php
{
    public class PhpPlatformDetectorResult : PlatformDetectorResult
    {
        public bool ComposerFileExists { get; set; }

        public bool ComposerLockFileExists { get; set; }

        public IDictionary<string, string> Dependencies { get; set; }
    }
}
