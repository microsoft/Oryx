using Microsoft.Oryx.BuildServer.Models;
using System;

namespace Microsoft.Oryx.BuildServer.Services.ArtifactBuilders
{
    public interface IArtifactBuilder
    {
        bool Build(Build build);
    }
}
