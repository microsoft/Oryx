// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.DeploymentProgress
{
    internal static class DeploymentProgressWriter
    {
        internal const int MaxFileBytes = 32 * 1024;
        internal const int MaxRecordBytes = 1024;
        internal const string EndpointEnvironmentVariable = "ORYX_PROGRESS_ENDPOINT";
        internal const string OperationIdEnvironmentVariable = "ORYX_PROGRESS_OPERATION_ID";

        private const int SchemaVersion = 1;
        private const int MaxOperationIdBytes = 256;
        private const string FileEndpointPrefix = "file:";
        private const int LinuxOpenWriteOnly = 0x1;
        private const int LinuxOpenNonBlocking = 0x800;
        private const int LinuxOpenDirectory = 0x10000;
        private const int LinuxOpenNoFollow = 0x20000;
        private const int LinuxOpenCloseOnExec = 0x80000;
        private const int LinuxOpenPath = 0x200000;
        private const int LinuxStatxNoFollowSymlink = 0x100;
        private const int LinuxStatxEmptyPath = 0x1000;
        private const uint LinuxStatxIdentity = 0x105;
        private const ushort LinuxFileTypeMask = 0xF000;
        private const ushort LinuxRegularFile = 0x8000;

        private static readonly Regex OperationIdRegex = new Regex(
            "^[A-Za-z0-9][A-Za-z0-9._:-]{0,255}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly ISet<string> Phases = new HashSet<string>(StringComparer.Ordinal)
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
                    !Phases.Contains(phase))
                {
                    return false;
                }

                var record = SerializeRecord(operationId, phase, timestampUtc);
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

        private static byte[] SerializeRecord(
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
            if (OperatingSystem.IsMacOS())
            {
                return false;
            }

            using (var stream = TryOpenEndpoint(endpointPath))
            {
                if (stream == null)
                {
                    return false;
                }

                var locked = false;
                try
                {
                    stream.Lock(0, MaxFileBytes);
                    locked = true;

                    if (stream.Length > MaxFileBytes - record.Length)
                    {
                        return false;
                    }

                    stream.Seek(0, SeekOrigin.End);
                    stream.Write(record, 0, record.Length);
                    stream.Flush(flushToDisk: true);
                    return true;
                }
                finally
                {
                    if (locked)
                    {
                        stream.Unlock(0, MaxFileBytes);
                    }
                }
            }
        }

        private static FileStream TryOpenEndpoint(string path)
        {
            if (OperatingSystem.IsLinux())
            {
                return TryOpenLinuxEndpoint(path);
            }

            if (!IsSafeExistingFile(path))
            {
                return null;
            }

            return new FileStream(
                path,
                FileMode.Open,
                FileAccess.Write,
                FileShare.ReadWrite,
                MaxRecordBytes,
                FileOptions.WriteThrough);
        }

        private static FileStream TryOpenLinuxEndpoint(string path)
        {
            var components = path.Split(
                Path.DirectorySeparatorChar,
                StringSplitOptions.RemoveEmptyEntries);
            if (components.Length == 0)
            {
                return null;
            }

            var directoryFileDescriptor = LinuxOpen(
                Path.DirectorySeparatorChar.ToString(),
                LinuxOpenPath | LinuxOpenDirectory | LinuxOpenNoFollow | LinuxOpenCloseOnExec);
            if (directoryFileDescriptor < 0)
            {
                return null;
            }

            try
            {
                for (var index = 0; index < components.Length - 1; index++)
                {
                    var nextDirectoryFileDescriptor = LinuxOpenAt(
                        directoryFileDescriptor,
                        components[index],
                        LinuxOpenPath | LinuxOpenDirectory | LinuxOpenNoFollow | LinuxOpenCloseOnExec);
                    if (nextDirectoryFileDescriptor < 0)
                    {
                        return null;
                    }

                    LinuxClose(directoryFileDescriptor);
                    directoryFileDescriptor = nextDirectoryFileDescriptor;
                }

                var fileDescriptor = LinuxOpenAt(
                    directoryFileDescriptor,
                    components[components.Length - 1],
                    LinuxOpenWriteOnly |
                        LinuxOpenNonBlocking |
                        LinuxOpenNoFollow |
                        LinuxOpenCloseOnExec);
                if (fileDescriptor < 0)
                {
                    return null;
                }

                var handle = new SafeFileHandle(new IntPtr(fileDescriptor), ownsHandle: true);
                if (!IsSafeLinuxEndpoint(
                    fileDescriptor,
                    directoryFileDescriptor,
                    components[components.Length - 1]))
                {
                    handle.Dispose();
                    return null;
                }

                try
                {
                    return new FileStream(handle, FileAccess.Write, MaxRecordBytes, isAsync: false);
                }
                catch
                {
                    handle.Dispose();
                    throw;
                }
            }
            finally
            {
                LinuxClose(directoryFileDescriptor);
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

        private static bool IsSafeExistingFile(string path)
        {
            var file = new FileInfo(path);
            file.Refresh();
            if (!file.Exists ||
                (file.Attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0)
            {
                return false;
            }

            var directory = file.Directory;
            while (directory != null)
            {
                directory.Refresh();
                if ((directory.Attributes & FileAttributes.ReparsePoint) != 0)
                {
                    return false;
                }

                directory = directory.Parent;
            }

            return true;
        }

        private static bool IsSafeLinuxEndpoint(
            int fileDescriptor,
            int directoryFileDescriptor,
            string fileName)
        {
            if (Statx(
                    fileDescriptor,
                    string.Empty,
                    LinuxStatxEmptyPath,
                    LinuxStatxIdentity,
                    out var openedFileStatus) != 0 ||
                Statx(
                    directoryFileDescriptor,
                    fileName,
                    LinuxStatxNoFollowSymlink,
                    LinuxStatxIdentity,
                    out var pathStatus) != 0)
            {
                return false;
            }

            return (openedFileStatus.Mode & LinuxFileTypeMask) == LinuxRegularFile &&
                openedFileStatus.LinkCount == 1 &&
                (pathStatus.Mode & LinuxFileTypeMask) == LinuxRegularFile &&
                pathStatus.LinkCount == 1 &&
                openedFileStatus.Inode == pathStatus.Inode &&
                openedFileStatus.DeviceMajor == pathStatus.DeviceMajor &&
                openedFileStatus.DeviceMinor == pathStatus.DeviceMinor;
        }

        [DllImport("libc", EntryPoint = "open", SetLastError = true)]
        private static extern int LinuxOpen(
            [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
            int flags);

        [DllImport("libc", EntryPoint = "openat", SetLastError = true)]
        private static extern int LinuxOpenAt(
            int directoryFileDescriptor,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
            int flags);

        [DllImport("libc", EntryPoint = "close", SetLastError = true)]
        private static extern int LinuxClose(int fileDescriptor);

        [DllImport("libc", EntryPoint = "statx", SetLastError = true)]
        private static extern int Statx(
            int directoryFileDescriptor,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
            int flags,
            uint mask,
            out LinuxStatx fileStatus);

        [StructLayout(LayoutKind.Explicit, Size = 256)]
        private struct LinuxStatx
        {
            [FieldOffset(16)]
            internal uint LinkCount;

            [FieldOffset(28)]
            internal ushort Mode;

            [FieldOffset(32)]
            internal ulong Inode;

            [FieldOffset(152)]
            internal uint DeviceMajor;

            [FieldOffset(156)]
            internal uint DeviceMinor;
        }
    }
}
