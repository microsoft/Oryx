// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Oryx.BuildServer.Respositories
{
    public class ServiceException : Exception
    {
        public ServiceException(string? message) : base(message)
        {
        }
    }

    public class IntegrityException : ServiceException
    {
        public IntegrityException(string? message) : base(message)
        {
        }
    }

    public class OperationFailedException : ServiceException
    {
        public OperationFailedException(string? message) : base(message)
        {
        }
    }
}