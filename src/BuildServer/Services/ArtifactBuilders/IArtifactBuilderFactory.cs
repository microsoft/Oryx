using Microsoft.Oryx.BuildServer.Models;

namespace Microsoft.Oryx.BuildServer.Services.ArtifactBuilders
{
    public interface IArtifactBuilderFactory
    {
        IArtifactBuilder CreateArtifactBuilder(Build build);
    }
}
