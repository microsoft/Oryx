// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.Integration.Tests
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
    }
}
