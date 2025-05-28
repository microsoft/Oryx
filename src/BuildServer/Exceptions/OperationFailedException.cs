// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildServer.Exceptions
{
    public class OperationFailedException : ServiceException
    {
        public OperationFailedException(string message)
            : base(message)
        {
        }
    }
}
