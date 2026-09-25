// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator.Common;

namespace Microsoft.Oryx.BuildScriptGenerator.DeploymentProgress
{
    internal sealed class DeploymentProgressReporter : IDeploymentProgressReporter
    {
        private static readonly Encoding Utf8Encoding = new UTF8Encoding(false, true);
        private static readonly ISet<string> LocalLinuxFilesystemTypes =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "aufs",
                "btrfs",
                "ext2",
                "ext3",
                "ext4",
                "overlay",
                "ramfs",
                "tmpfs",
                "xfs",
                "zfs",
            };

        private static readonly Regex OperationIdRegex = new Regex(
            DeploymentProgressConstants.OperationIdPattern,
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ECMAScript);

        private readonly Func<DateTimeOffset> utcNow;
        private readonly Action<byte[]> appendRecord;
        private readonly string operationId;
        private bool buildStarted;
        private bool buildCompleted;
        private bool enabled;
        private int eventCount;
        private int lastPhaseRank;

        public DeploymentProgressReporter(
            IEnvironment environment,
            ILogger<DeploymentProgressReporter> logger)
            : this(
                  environment.GetEnvironmentVariable(DeploymentProgressConstants.EndpointEnvironmentVariable),
                  environment.GetEnvironmentVariable(DeploymentProgressConstants.OperationIdEnvironmentVariable),
                  logger,
                  () => DateTimeOffset.UtcNow,
                  appendRecord: null,
                  processFactory: null)
        {
        }

        internal DeploymentProgressReporter(
            string endpoint,
            string operationId,
            ILogger<DeploymentProgressReporter> logger,
            Func<DateTimeOffset> utcNow,
            Action<byte[]> appendRecord,
            IDeploymentProgressWriterProcessFactory processFactory = null)
        {
            this.utcNow = utcNow;

            if (string.IsNullOrEmpty(endpoint))
            {
                return;
            }

            try
            {
                if (!TryGetFilePath(endpoint, out var filePath) ||
                    !IsOperationIdValid(operationId))
                {
                    this.Disable("Deployment progress reporting configuration is invalid.");
                    return;
                }

                this.operationId = operationId;
                this.appendRecord = appendRecord ??
                    new BoundedDeploymentProgressWriter(filePath, processFactory).AppendRecord;
                this.enabled = true;
            }
            catch (Exception)
            {
                this.Disable("Deployment progress reporting initialization failed.");
            }
        }

        public bool IsEnabled => this.enabled;

        public void ReportBuildStarted()
        {
            if (!this.enabled || this.buildStarted)
            {
                return;
            }

            this.WriteEvent("build_started");
            this.buildStarted = this.enabled;
        }

        public void ReportPhaseStarted(string phase)
        {
            if (!this.enabled || !this.buildStarted || this.buildCompleted)
            {
                return;
            }

            if (!DeploymentProgressConstants.PhaseRanks.TryGetValue(phase, out var rank))
            {
                this.Disable("Deployment progress reporting received an invalid phase.");
                return;
            }

            if (rank <= this.lastPhaseRank)
            {
                return;
            }

            this.WriteEvent("phase_started", writer => writer.WriteString("phase", phase));
            if (this.enabled)
            {
                this.lastPhaseRank = rank;
            }
        }

        public void ReportBuildCompleted(string outcome)
        {
            if (!this.enabled || !this.buildStarted || this.buildCompleted)
            {
                return;
            }

            if (!DeploymentProgressConstants.BuildOutcomes.Contains(outcome))
            {
                this.Disable("Deployment progress reporting received an invalid build outcome.");
                return;
            }

            this.WriteEvent(
                "build_completed",
                writer => writer.WriteString("outcome", outcome));
            this.buildCompleted = true;
        }

        internal static int AppendRecordFromStandardInput(Stream input, string endpoint)
        {
            if (input == null || !TryGetFilePath(endpoint, out var filePath))
            {
                return ProcessConstants.ExitFailure;
            }

            var buffer = new byte[DeploymentProgressConstants.MaxRecordBytes + 1];
            var length = 0;
            while (length < buffer.Length)
            {
                var bytesRead = input.Read(buffer, length, buffer.Length - length);
                if (bytesRead == 0)
                {
                    break;
                }

                length += bytesRead;
            }

            if (length == 0 ||
                length > DeploymentProgressConstants.MaxRecordBytes ||
                buffer[length - 1] != (byte)'\n')
            {
                return ProcessConstants.ExitFailure;
            }

            var record = new byte[length];
            Buffer.BlockCopy(buffer, 0, record, 0, length);
            if (!IsValidSerializedRecord(record))
            {
                return ProcessConstants.ExitFailure;
            }

            try
            {
                AppendRecord(filePath, record);
                return ProcessConstants.ExitSuccess;
            }
            catch
            {
                return ProcessConstants.ExitFailure;
            }
        }

        private static bool IsValidSerializedRecord(byte[] record)
        {
            try
            {
                using (var document = JsonDocument.Parse(record))
                {
                    var root = document.RootElement;
                    if (root.ValueKind != JsonValueKind.Object)
                    {
                        return false;
                    }

                    var properties = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
                    foreach (var property in root.EnumerateObject())
                    {
                        if (!properties.TryAdd(property.Name, property.Value))
                        {
                            return false;
                        }
                    }

                    if (!TryGetInt32(properties, "schemaVersion", out var schemaVersion) ||
                        schemaVersion != DeploymentProgressConstants.SchemaVersion ||
                        !TryGetString(properties, "eventType", out var eventType) ||
                        !TryGetString(properties, "operationId", out var operationId) ||
                        !IsOperationIdValid(operationId) ||
                        !TryGetString(properties, "timestampUtc", out var timestamp) ||
                        !DateTimeOffset.TryParse(
                            timestamp,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.RoundtripKind,
                            out _))
                    {
                        return false;
                    }

                    var allowedProperties = new HashSet<string>(StringComparer.Ordinal)
                    {
                        "schemaVersion",
                        "eventType",
                        "operationId",
                        "timestampUtc",
                    };

                    switch (eventType)
                    {
                        case "build_started":
                            break;
                        case "phase_started":
                            allowedProperties.Add("phase");
                            if (!TryGetString(properties, "phase", out var phase) ||
                                !DeploymentProgressConstants.PhaseRanks.ContainsKey(phase))
                            {
                                return false;
                            }

                            break;
                        case "build_completed":
                            allowedProperties.Add("outcome");
                            if (!TryGetString(properties, "outcome", out var outcome) ||
                                !DeploymentProgressConstants.BuildOutcomes.Contains(outcome))
                            {
                                return false;
                            }

                            break;
                        default:
                            return false;
                    }

                    return properties.Keys.All(allowedProperties.Contains);
                }
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private static bool TryGetInt32(
            IDictionary<string, JsonElement> properties,
            string name,
            out int value)
        {
            value = 0;
            return properties.TryGetValue(name, out var property) &&
                property.ValueKind == JsonValueKind.Number &&
                property.TryGetInt32(out value);
        }

        private static bool TryGetString(
            IDictionary<string, JsonElement> properties,
            string name,
            out string value)
        {
            value = null;
            if (!properties.TryGetValue(name, out var property) ||
                property.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            value = property.GetString();
            return true;
        }

        private static void AppendRecord(string filePath, byte[] record)
        {
            RejectUnsafePath(filePath);

            using (var stream = new FileStream(
                filePath,
                FileMode.Append,
                FileAccess.Write,
                FileShare.Read,
                DeploymentProgressConstants.MaxRecordBytes,
                FileOptions.WriteThrough))
            {
                RejectUnsafePath(filePath);
                if (!stream.CanSeek ||
                    stream.Length + record.Length > DeploymentProgressConstants.MaxFileBytes)
                {
                    throw new IOException("Deployment progress endpoint is unavailable.");
                }

                stream.Write(record, 0, record.Length);
                stream.Flush(flushToDisk: true);
            }
        }

        private static void RejectUnsafePath(string filePath)
        {
            var file = new FileInfo(filePath);
            file.Refresh();
            if (!file.Exists ||
                file.LinkTarget != null ||
                (file.Attributes &
                    (FileAttributes.Device | FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0)
            {
                throw new IOException("Deployment progress endpoint is not a regular file.");
            }

            var directory = file.Directory;
            while (directory != null)
            {
                directory.Refresh();
                if (directory.LinkTarget != null ||
                    (directory.Attributes & FileAttributes.ReparsePoint) != 0)
                {
                    throw new IOException("Deployment progress endpoint has an unsupported ancestor.");
                }

                directory = directory.Parent;
            }

            if (OperatingSystem.IsWindows())
            {
                var root = Path.GetPathRoot(filePath);
                if (filePath.StartsWith(@"\\", StringComparison.Ordinal) ||
                    (!string.IsNullOrEmpty(root) && new DriveInfo(root).DriveType == DriveType.Network))
                {
                    throw new IOException("Deployment progress endpoint is not local.");
                }
            }
            else if (OperatingSystem.IsLinux())
            {
                if (!IsLocalLinuxFilesystem(filePath))
                {
                    throw new IOException("Deployment progress endpoint is not local.");
                }
            }
            else
            {
                throw new PlatformNotSupportedException(
                    "Deployment progress endpoints require Windows or Linux.");
            }
        }

        private static bool IsLocalLinuxFilesystem(string filePath)
        {
            var directoryPath = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(directoryPath))
            {
                return false;
            }

            string matchingFilesystemType = null;
            var matchingMountPointLength = -1;
            foreach (var line in File.ReadLines("/proc/self/mountinfo"))
            {
                var separatorIndex = line.IndexOf(" - ", StringComparison.Ordinal);
                if (separatorIndex < 0)
                {
                    continue;
                }

                var mountFields = line.Substring(0, separatorIndex).Split(' ');
                var filesystemFields = line.Substring(separatorIndex + 3).Split(' ');
                if (mountFields.Length < 5 || filesystemFields.Length == 0)
                {
                    continue;
                }

                var mountPoint = DecodeMountInfoPath(mountFields[4]);
                if (mountPoint.Length <= matchingMountPointLength ||
                    !IsPathWithinMount(directoryPath, mountPoint))
                {
                    continue;
                }

                matchingMountPointLength = mountPoint.Length;
                matchingFilesystemType = filesystemFields[0];
            }

            return matchingFilesystemType != null &&
                LocalLinuxFilesystemTypes.Contains(matchingFilesystemType);
        }

        private static bool IsPathWithinMount(string path, string mountPoint)
        {
            if (mountPoint == Path.DirectorySeparatorChar.ToString())
            {
                return true;
            }

            var normalizedMountPoint = mountPoint.TrimEnd(Path.DirectorySeparatorChar);
            return string.Equals(path, normalizedMountPoint, StringComparison.Ordinal) ||
                path.StartsWith(
                    normalizedMountPoint + Path.DirectorySeparatorChar,
                    StringComparison.Ordinal);
        }

        private static string DecodeMountInfoPath(string value)
        {
            return value
                .Replace(@"\040", " ")
                .Replace(@"\011", "\t")
                .Replace(@"\012", "\n")
                .Replace(@"\134", @"\");
        }

        private static bool TryGetFilePath(string endpoint, out string filePath)
        {
            filePath = null;
            if (string.IsNullOrEmpty(endpoint) ||
                !endpoint.StartsWith(
                    DeploymentProgressConstants.FileEndpointPrefix,
                    StringComparison.Ordinal))
            {
                return false;
            }

            var candidate = endpoint.Substring(DeploymentProgressConstants.FileEndpointPrefix.Length);
            if (string.IsNullOrEmpty(candidate) ||
                candidate.Any(char.IsControl) ||
                !Path.IsPathFullyQualified(candidate) ||
                (OperatingSystem.IsWindows() && candidate.StartsWith(@"\\", StringComparison.Ordinal)))
            {
                return false;
            }

            filePath = Path.GetFullPath(candidate);
            return true;
        }

        private static bool IsOperationIdValid(string operationId)
        {
            if (string.IsNullOrEmpty(operationId) ||
                Utf8Encoding.GetByteCount(operationId) > DeploymentProgressConstants.MaxOperationIdBytes)
            {
                return false;
            }

            var match = OperationIdRegex.Match(operationId);
            return match.Success && match.Length == operationId.Length;
        }

        private void WriteEvent(string eventType, Action<Utf8JsonWriter> writeEventFields = null)
        {
            if (!this.enabled)
            {
                return;
            }

            if (this.eventCount >= DeploymentProgressConstants.MaxEventsPerStream)
            {
                this.Disable("Deployment progress reporting reached its event limit.");
                return;
            }

            try
            {
                var buffer = new ArrayBufferWriter<byte>(DeploymentProgressConstants.MaxRecordBytes);
                using (var writer = new Utf8JsonWriter(buffer))
                {
                    writer.WriteStartObject();
                    writer.WriteNumber("schemaVersion", DeploymentProgressConstants.SchemaVersion);
                    writer.WriteString("eventType", eventType);
                    writer.WriteString("operationId", this.operationId);
                    writer.WriteString("timestampUtc", this.utcNow().UtcDateTime);
                    writeEventFields?.Invoke(writer);
                    writer.WriteEndObject();
                    writer.Flush();
                }

                var recordLength = buffer.WrittenCount + 1;
                if (recordLength > DeploymentProgressConstants.MaxRecordBytes)
                {
                    this.Disable("Deployment progress reporting produced an oversized event.");
                    return;
                }

                var record = new byte[recordLength];
                buffer.WrittenSpan.CopyTo(record);
                record[record.Length - 1] = (byte)'\n';
                this.appendRecord(record);
                this.eventCount++;
            }
            catch (Exception)
            {
                this.Disable("Deployment progress reporting failed and was disabled.");
            }
        }

        private void Disable(string message)
        {
            this.enabled = false;
            System.Diagnostics.Debug.WriteLine(message);
        }
    }
}
