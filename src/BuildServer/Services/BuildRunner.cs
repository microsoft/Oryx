// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildServer.Models;
using Microsoft.Oryx.BuildServer.Services.ArtifactBuilders;

namespace Microsoft.Oryx.BuildServer.Services
{
    public class BuildRunner : IBuildRunner
    {
        public void RunInBackground(IArtifactBuilder builder, Build build, Callback successCallback, Callback failureCallback)
        {
            _ = Task.Run(() =>
                {
                    try
                    {
                        if (builder.Build(build))
                        {
                            _ = successCallback(build);
                        }
                        else
                        {
                            _ = failureCallback(build);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                });
        }
    }
}
