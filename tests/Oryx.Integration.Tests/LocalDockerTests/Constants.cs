// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.Integration.Tests.LocalDockerTests
{
    internal class Constants
    {
        // Common database settings. Use these settings for all database related tests and also make sure
        // the sample apps use these to connect.

        // The name of the link to the running database server container. This link name is supposed to be
        // used from within the sample app(which runs in a different container).
        public const string InternalDbLinkName = "dbserver";
        public const string DatabaseName = "oryxdb";
        public const string DatabaseUserName = "oryxuser";
        public static readonly string DatabaseUserPwd = System.Guid.NewGuid().ToString();

        internal const int NodeEndToEndTestsPort = 8010;
        internal const int PythonEndToEndTestsPort = NodeEndToEndTestsPort + 10;
        internal const int DotNetCoreEndToEndTestsPort = PythonEndToEndTestsPort + 10;
        internal const int PhpEndToEndTestsPort = DotNetCoreEndToEndTestsPort + 10;
    }
}
