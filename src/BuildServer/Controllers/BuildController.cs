// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Oryx.BuildServer.Exceptions;
using Microsoft.Oryx.BuildServer.Models;
using Microsoft.Oryx.BuildServer.Services;

namespace Microsoft.Oryx.BuildServer.Controllers
{
    [Route("api/builds")]
    [ApiController]
    public class BuildController : ControllerBase
    {
        private readonly IBuildService buildService;

        public BuildController(IBuildService buildService)
        {
            this.buildService = buildService;
        }

        // GET api/<Builds>/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Build), StatusCodes.Status201Created)]
        public async Task<IActionResult> GetAsync(string id)
        {
            var build = await this.buildService.GetBuildAsync(id);
            if (build == null)
            {
                return this.NotFound();
            }

            return this.Ok(build);
        }

        // POST api/<Builds>
        [HttpPost]
        [ProducesResponseType(typeof(Build), StatusCodes.Status201Created)]
        public async Task<IActionResult> PostAsync([FromBody] Build build)
        {
            try
            {
                var createdBuild = await this.buildService.StartBuildAsync(build);
                string uri = string.Format("api/builds/{0}", createdBuild.Id);
                return this.Created(uri, createdBuild);
            }
            catch (ServiceException ex)
            {
                return this.BadRequest(ex.Message);
            }
        }
    }
}
