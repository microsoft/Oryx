// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonFlatFileDataStore;
using Microsoft.Oryx.BuildServer.Exceptions;
using Microsoft.Oryx.BuildServer.Models;

namespace Microsoft.Oryx.BuildServer.Repositories
{
    public class BuildRepository : IRepository
    {
        private readonly DataStore store;
        private readonly IDocumentCollection<Build> collection;

        public BuildRepository(DataStore store)
        {
            this.store = store;
            this.collection = this.store.GetCollection<Build>();
        }

#pragma warning disable CS1998 // Keep asynchronous for backwards-compatibility
        public async Task<IEnumerable<Build>> GetAllAsync()
#pragma warning restore CS1998
        {
            return this.collection.Find(x => true);
        }

        public Build GetById(string id)
        {
            var build = this.collection.Find(x => x.Id == id).FirstOrDefault();
            return build;
        }

        public async Task<Build> InsertAsync(Build build)
        {
            if (this.GetById(build.Id) != null)
            {
                throw new IntegrityException(string.Format("Build with id {0} already present", build.Id));
            }

            if (await this.collection.ReplaceOneAsync(build.Id, build, true))
            {
                return build;
            }

            throw new OperationFailedException("Insert Failed");
        }

        public async Task<Build> UpdateAsync(Build build)
        {
            if (await this.collection.UpdateOneAsync(build.Id, build))
            {
                return build;
            }

            throw new OperationFailedException("Update Failed");
        }
    }
}
