// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildServer.Models;
using Microsoft.Oryx.BuildServer.Services.ArtifactBuilders;
using System;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildServer.Services
{
    public class BuildRunner : IBuildRunner
    {
        public void RunInBackground(IArtifactBuilder builder, Build build, Callback successCallback, Callback failureCallback)
        {
            var t = Task.Run(() =>
                {
                    try
                    {
                        if(builder.Build(build))
                        {
                            successCallback(build);
                        } else
                        {
                            failureCallback(build);
                        }
                    } catch (Exception ex) {
                        Console.WriteLine(ex);
                    }
                }
            );
        }
    }
}
