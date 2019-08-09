// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Newtonsoft.Json;

// Classes in this file are adapted from:
// https://github.com/microsoft/vscode-debugadapter-node/blob/master/protocol/src/debugProtocol.ts
namespace Microsoft.Oryx.Integration.Tests.VSCodeDebugProtocol.Messages
{
    public class ProtocolMessage
    {
        [JsonProperty("seq")]
        public uint SequenceNumber { get; set; }

        [JsonProperty("type")]
        public virtual string Type { get; set; } // 'request', 'response', 'event', etc.
    }

    public class Request<T> : ProtocolMessage
    {
        public override string Type => "request";

        [JsonProperty("command")]
        public virtual string Command { get; set; }

        [JsonProperty("arguments")]
        public virtual T Args { get; set; }
    }

    public class InitializeRequest : Request<InitializeRequestArguments>
    {
        public override string Command => "initialize";
    }

    public class InitializeRequestArguments
    {
        [JsonProperty("clientID")]
        public string ClientID { get; set; }

        [JsonProperty("clientName")]
        public string ClientName { get; set; }

        [JsonProperty("adapterID")]
        public string AdapterID { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("linesStartAt1")]
        public bool LinesStartAt1 { get; set; }

        [JsonProperty("columnsStartAt1")]
        public bool ColumnsStartAt1 { get; set; }

        [JsonProperty("pathFormat")]
        public string PathFormat { get; set; } // 'path', 'uri', etc.

        [JsonProperty("supportsVariableType")]
        public bool SupportsVariableType { get; set; }

        [JsonProperty("supportsVariablePaging")]
        public bool SupportsVariablePaging { get; set; }

        [JsonProperty("supportsRunInTerminalRequest")]
        public bool SupportsRunInTerminalRequest { get; set; }

        [JsonProperty("supportsMemoryReferences")]
        public bool SupportsMemoryReferences { get; set; }
    }
}