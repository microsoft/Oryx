// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildServer.Models;

namespace Microsoft.Oryx.BuildServer.Repositories
{
    public interface IRepository
    {
        public Task<Build> InsertAsync(Build build);

        public Task<IEnumerable<Build>> GetAllAsync();

        public Task<Build> UpdateAsync(Build build);

        public Build GetById(string id);
    }
}
