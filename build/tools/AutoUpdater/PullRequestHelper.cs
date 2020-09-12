// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Text;

namespace AutoUpdater
{
    internal class PullRequestHelper
    {
        public static string GetDescriptionForCreatingPullRequest(string forkBranchName)
        {
            var description = new StringBuilder();
            description.AppendJoin(
                separator: @"\n",
                $"git fetch oryxci {forkBranchName}",
                $"git checkout {forkBranchName}",
                "git fetch origin",
                "git rebase origin/master",
                $"git push -u origin {forkBranchName}");
            return description.ToString();
        }
    }
}
