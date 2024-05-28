// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.Node;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal static class ProcessExitCodeHelper
    {
        public static int GetExitCodeForException(Exception exception)
        {
            // NOTE: Some partners depend on specific exit codes to take alternative actions at their end,
            // so make sure to let me know if the behavior here is changed.
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            if (exception is UnsupportedPlatformException)
            {
                return ProcessConstants.UnsupportedPlatform;
            }

            if (exception is UnsupportedVersionException)
            {
                return ProcessConstants.UnsupportedPlatformVersion;
            }

            if (exception is NoBuildStepException)
            {
                return ProcessConstants.NoBuildStepException;
            }

            return ProcessConstants.ExitFailure;
        }
    }
}
