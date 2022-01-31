using JsonFlatFileDataStore;
using Microsoft.Oryx.BuildServer.Models;
using Microsoft.Oryx.BuildServer.Respositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildServer.Repositories
{
    public class BuildRepository : IRepository
    {
        private readonly DataStore _store;
        private readonly IDocumentCollection<Build> _collection;
        public BuildRepository(DataStore store)
        {
            _store = store;
            _collection = _store.GetCollection<Build>();
        }

        public async Task<IEnumerable<Build>> GetAll()
        {
            return _collection.Find(x => true);
        }

        public Build? GetById(string id)
        {
            var build = _collection.Find(x => x.Id == id).FirstOrDefault();
            return build;
        }

        public async Task<Build> Insert(Build build)
        {
            if (GetById(build.Id) != null)
            {
                throw new IntegrityException(String.Format("Build with id {0} already present", build.Id));
            }
            if (await _collection.ReplaceOneAsync(build.Id, build, true))
                return build;
            throw new OperationFailedException("Insert Failed");
        }

        public async Task<Build> Update(Build build)
        {
            if(await _collection.UpdateOneAsync(build.Id, build))
                return build;
            throw new OperationFailedException("Update Failed");
        }
    }
}
