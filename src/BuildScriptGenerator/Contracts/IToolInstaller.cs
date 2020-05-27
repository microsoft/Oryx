using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Oryx.BuildScriptGenerator.Contracts
{
    public interface IToolInstaller
    {
        string ToolName { get; }

        string GetInstallerScriptSnippet(string version);

        bool IsVersionAlreadyInstalled(string version);
    }
}
