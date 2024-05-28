using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Oryx.BuildScriptGenerator.Golang
{
    internal interface IGolangVersionProvider
    {
        PlatformVersionInfo GetVersionInfo();
    }
}
