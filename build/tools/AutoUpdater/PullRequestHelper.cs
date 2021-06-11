// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Text;

namespace AutoUpdater
{
    internal class PullRequestHelper
    {
        public static string GetDescriptionForCreatingPullRequest(string forkAccountName, string forkBranchName)
        {
            var description = new StringBuilder();
            description.AppendJoin(
                separator: @"\n",
                $"git remote add oryxci https://github.com/{forkAccountName}/oryx || true",
                $"git fetch oryxci {forkBranchName}",
                $"git checkout {forkBranchName}",
                "git fetch origin",
                "git rebase origin/main",
                $"git push -u origin {forkBranchName}");
            return description.ToString();
        }
    }
}
