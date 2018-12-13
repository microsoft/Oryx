using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Oryx.Common.Utilities
{
    public static class LoggingConstants
    {
        public const string AppServiceAppNameEnvironmentVariableName = "APPSETTING_WEBSITE_SITE_NAME";

        public static readonly TimeSpan FlushTimeout = TimeSpan.FromSeconds(3);
    }
}
