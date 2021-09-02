// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Oryx.BuildServer
{
    enum BuildState
    {
        Success,
        InProcess,
        Failed
    }
    public class BuildResponse
    {
        public int StatusCode { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }

        public BuildResponse(int code, string status, string message)
        {
            StatusCode = code;
            Status = status;
            Message = message;
        }

        public BuildResponse()
        {
        }
    }
}
