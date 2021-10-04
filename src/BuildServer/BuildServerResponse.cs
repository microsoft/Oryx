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
        Building,
        Failed,
        InvalidRequestParameter,
        Unknown
    }

    public class StatusUrl
    {
        public string BuildStatusCheckUrl { get; set; }
        public string ServerStatusCheckUrl { get; set; }

        public StatusUrl(StatusUrl urls)
        {
            BuildStatusCheckUrl = urls.BuildStatusCheckUrl;
            ServerStatusCheckUrl = urls.ServerStatusCheckUrl;
        }

        public StatusUrl(string buildUrl, string serverUrl)
        {
            BuildStatusCheckUrl = buildUrl;
            ServerStatusCheckUrl = serverUrl;
        }

        public StatusUrl()
        { 
        }
    }

    public class BuildOutput
    {
        public string StandardOut { get; set; }
        public string StandardError { get; set; }

        public BuildOutput(BuildOutput buildOutput)
        {
            StandardOut = buildOutput.StandardOut;
            StandardError = buildOutput.StandardError;
        }

        public BuildOutput(string standardOut, string standardError)
        {
            StandardOut = standardOut;
            StandardError = standardError;
        }
    }

    public class BuildServerResponse
    {
        public int StatusCode { get; set; }
        public string Status { get; set; }
        public StatusUrl StatusCheckUrl { get; set; }
        public BuildOutput Message { get; set; }

        public BuildServerResponse(int code, string status, StatusUrl urls, BuildOutput output)
        {
            StatusCode = code;
            Status = status;
            StatusCheckUrl = urls;
            Message = output;
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
