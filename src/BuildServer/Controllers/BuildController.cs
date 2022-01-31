using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Oryx.BuildServer.Models;
using Microsoft.Oryx.BuildServer.Respositories;
using Microsoft.Oryx.BuildServer.Services;
using System;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Microsoft.Oryx.BuildServer.Controllers
{
    [Route("api/builds")]
    [ApiController]
    public class BuildController : ControllerBase
    {
        private readonly IBuildService _buildService;

        public BuildController(IBuildService buildService)
        {
            _buildService = buildService;
        }

        // GET api/<Builds>/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Build), StatusCodes.Status201Created)]
        public async Task<IActionResult> Get(string id)
        {
            var build = await _buildService.GetBuild(id);
            if (build == null)
            {
                return NotFound();
            }
            return Ok(build);
        }

        // POST api/<Builds>
        [HttpPost]
        [ProducesResponseType(typeof(Build), StatusCodes.Status201Created)]
        public async Task<IActionResult> Post([FromBody] Build build)
        {
            try
            {
                var createdBuild = await _buildService.StartBuild(build);
                string uri = String.Format("api/builds/{0}", createdBuild.Id);
                return Created(uri, createdBuild);
            }
            catch (ServiceException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT api/<Builds>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] Build build)
        {
        }
    }
}
