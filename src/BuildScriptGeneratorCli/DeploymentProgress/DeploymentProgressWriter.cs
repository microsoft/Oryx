// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.DeploymentProgress
{
    internal static class DeploymentProgressWriter
    {
        internal const int MaxFileBytes = 32 * 1024;
        internal const int MaxRecordBytes = 1024;

        private const int SchemaVersion = 1;
        private const int MaxOperationIdBytes = 256;
        private const string EndpointEnvironmentVariable = "ORYX_PROGRESS_ENDPOINT";
        private const string OperationIdEnvironmentVariable = "ORYX_PROGRESS_OPERATION_ID";
        private const string FileEndpointPrefix = "file:";

        private static readonly Regex OperationIdRegex = new Regex(
            "^[A-Za-z0-9][A-Za-z0-9._:-]{0,255}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly ISet<string> AllowedPhases = new HashSet<string>(StringComparer.Ordinal)
        {
            "pre_build",
            "build.execute",
            "post_build",
            "output.prepare",
            "output.compress",
            "manifest.write",
        };

        internal static bool TryAppendPhaseFromEnvironment(string phase)
        {
            return TryAppendPhase(
                Environment.GetEnvironmentVariable(EndpointEnvironmentVariable),
                Environment.GetEnvironmentVariable(OperationIdEnvironmentVariable),
                phase,
                DateTimeOffset.UtcNow);
        }

        internal static bool TryAppendPhase(
            string endpoint,
            string operationId,
            string phase,
            DateTimeOffset timestampUtc)
        {
            try
            {
                if (!TryGetEndpointPath(endpoint, out var endpointPath) ||
                    !IsValidOperationId(operationId) ||
                    !AllowedPhases.Contains(phase))
                {
                    return false;
                }

                var record = SerializePhaseStartedRecord(operationId, phase, timestampUtc);
                if (record.Length > MaxRecordBytes)
                {
                    return false;
                }

                return TryAppendRecord(endpointPath, record);
            }
            catch
            {
                return false;
            }
        }

        private static byte[] SerializePhaseStartedRecord(
            string operationId,
            string phase,
            DateTimeOffset timestampUtc)
        {
            var buffer = new ArrayBufferWriter<byte>(MaxRecordBytes);
            using (var writer = new Utf8JsonWriter(buffer))
            {
                writer.WriteStartObject();
                writer.WriteNumber("schemaVersion", SchemaVersion);
                writer.WriteString("eventType", "phase_started");
                writer.WriteString("operationId", operationId);
                writer.WriteString("phase", phase);
                writer.WriteString(
                    "timestampUtc",
                    timestampUtc.UtcDateTime.ToString("O", CultureInfo.InvariantCulture));
                writer.WriteEndObject();
                writer.Flush();
            }

            var record = new byte[buffer.WrittenCount + 1];
            buffer.WrittenSpan.CopyTo(record);
            record[record.Length - 1] = (byte)'\n';
            return record;
        }

        private static bool TryAppendRecord(string endpointPath, byte[] record)
        {
            if (!File.Exists(endpointPath))
            {
                return false;
            }

            using (var stream = new FileStream(
                endpointPath,
                FileMode.Open,
                FileAccess.Write,
                FileShare.ReadWrite | FileShare.Delete))
            {
                if (stream.Length > MaxFileBytes - record.Length)
                {
                    return false;
                }

                stream.Seek(0, SeekOrigin.End);
                stream.Write(record, 0, record.Length);
                stream.Flush();
                return true;
            }
        }

        private static bool TryGetEndpointPath(string endpoint, out string endpointPath)
        {
            endpointPath = null;
            if (string.IsNullOrEmpty(endpoint) ||
                !endpoint.StartsWith(FileEndpointPrefix, StringComparison.Ordinal))
            {
                return false;
            }

            var candidate = endpoint.Substring(FileEndpointPrefix.Length);
            if (string.IsNullOrEmpty(candidate) ||
                !Path.IsPathFullyQualified(candidate) ||
                ContainsControlCharacter(candidate))
            {
                return false;
            }

            endpointPath = Path.GetFullPath(candidate);
            return true;
        }

        private static bool IsValidOperationId(string operationId)
        {
            if (string.IsNullOrEmpty(operationId) ||
                Encoding.UTF8.GetByteCount(operationId) > MaxOperationIdBytes)
            {
                return false;
            }

            var match = OperationIdRegex.Match(operationId);
            return match.Success && match.Length == operationId.Length;
        }

        private static bool ContainsControlCharacter(string value)
        {
            foreach (var character in value)
            {
                if (char.IsControl(character))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
