// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Oryx.Detector.Exceptions
{
    /// <summary>
    /// A generic xception used by Detector, as opposed to system ones.
    /// Its message must be user friendly, and can be displayed directly to the user.
    /// </summary>
    public class DetectorException : Exception
    {
        public DetectorException(string message)
            : base(message)
        {
        }
    }
}