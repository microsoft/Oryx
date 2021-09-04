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
        Running,
        Failed
    }
    public class BuildServerResponse
    {
        public int StatusCode { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }

        public BuildServerResponse(int code, string status, string message)
        {
            StatusCode = code;
            Status = status;
            Message = message;
        }

        public BuildServerResponse()
        {
        }
    }

    public class ErrorResponse
    {
        public string Type { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }

        public ErrorResponse(Exception ex)
        {
            Type = ex.GetType().Name;
            Message = ex.Message;
            StackTrace = ex.ToString();
        }

        public ErrorResponse()
        {
        }
    }
}
