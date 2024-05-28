// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Oryx.BuildServer.Exceptions
{
    public class ServiceException : Exception
    {
        public ServiceException(string message)
            : base(message)
        {
        }
    }
}
