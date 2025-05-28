// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildServer.Exceptions
{
    public class IntegrityException : ServiceException
    {
        public IntegrityException(string message)
            : base(message)
        {
        }
    }
}
