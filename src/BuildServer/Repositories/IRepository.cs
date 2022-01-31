using Microsoft.Oryx.BuildServer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildServer.Repositories
{
    public interface IRepository
    {
        public Task<Build> Insert(Build build);
        public Task<IEnumerable<Build>> GetAll();
        public Task<Build> Update(Build build);
        public Build? GetById(string id);
    }
}
