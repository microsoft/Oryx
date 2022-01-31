using Microsoft.Oryx.BuildServer.Models;
using Microsoft.Oryx.BuildServer.Services.ArtifactBuilders;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildServer.Services
{
    public delegate Task<Build> Callback(Build build);

    public interface IBuildRunner
    {
        void RunInBackground(IArtifactBuilder builder, Build build, Callback successCallback, Callback FailureCallback);
    }
}
