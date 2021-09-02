// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuildController : Controller
    {
        private readonly ILogger<BuildController> _logger;

        public BuildController(ILogger<BuildController> logger)
        {
            _logger = logger;
        }

        [Route("/build/Get")]
        [Route("[controller]/Get")]
        ////[Route("[controller]/[action]")]
        [HttpGet]
        // 
        // GET: /build/Get
        public int Get()
        {
            return 200;
        }

        [Route("[controller]/[action]")]
        [Route("/build/CheckServerStatus")]
        [Produces("application/json")]
        [HttpGet]
        // 
        // GET: /build/CheckServerStatus/ 
        public async Task<IActionResult> CheckServerStatus()
        {
            int exitCode = 0;
            string output = string.Empty;
            string error = string.Empty;
            
            var script = new ShellScriptBuilder()
                        .AddCommand("oryx --version").ToString();
            (exitCode, output, error) = await Task.Run(() => RunOryxCommand(script)).ConfigureAwait(false);

            return StatusCode((int)HttpStatusCode.OK, output);
        }

        [Route("/build/[action]")]
        [Route("[controller]/CheckBuildStatus")]
        [Produces("application/json")]
        // 
        // GET: /build/CheckBuildStatus/ 
        public async Task<IActionResult> CheckBuildStatus(string destination, string logFileName)
        {
            int exitCode = 0;
            string output = string.Empty;
            string error = string.Empty;

            if (string.IsNullOrEmpty(destination) || string.IsNullOrEmpty(logFileName))
            {
                _logger.LogError("Destination and/or build logfile name is empty in requestbody.");
                return BadRequest();
            }

            var buildManifestFilePath = Path.Join(destination, FilePaths.BuildManifestFileName);
            var buildLogFilePath = Path.Join(destination, logFileName);

            // Checking for a scenario where build log and manifest file
            // both exists. This means it's a successful build scenario
            // http response: 200

            var script = new ShellScriptBuilder()
                .AddFileExistsCheck(buildManifestFilePath)
                .AddFileExistsCheck(buildLogFilePath)
                .ToString();
            try
            {
                (exitCode, output, error) = await Task.Run(() => RunOryxCommand(script)).ConfigureAwait(false);
                _logger.LogDebug($"exitcode {exitCode} and output: {output}");
                if (exitCode == 0)
                {
                    var response = new BuildResponse();
                    response.Message = buildManifestFilePath;
                    response.Status = BuildState.Success.ToString();
                    response.StatusCode = (int)HttpStatusCode.OK;
                    return StatusCode((int)HttpStatusCode.OK, response);
                }

            }
            catch (Exception ex)
            {
                // Checking for a scenario where build log exists but
                // manifest file doesn't. This means it's a failed build scenario
                // http response: 500

                _logger.LogError(ex.Message);
                script = new ShellScriptBuilder()
                .AddFileDoesNotExistCheck(buildManifestFilePath)
                .AddFileExistsCheck(buildLogFilePath)
                .ToString();
                try
                {
                    _logger.LogInformation("Checking if manifestfile doesn't exist but build log exists");
                    (exitCode, output, error) = await Task.Run(() => RunOryxCommand(script)).ConfigureAwait(false);
                    if (exitCode == 0)
                    {
                        var response = new BuildResponse();
                        response.Message = "Build failed, Build Log exists but manifest file doesn't.";
                        response.Status = BuildState.Failed.ToString();
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        return StatusCode((int)HttpStatusCode.InternalServerError, response);
                    }
                }
                catch (Exception ePending)
                {
                    // Checking for a scenario where build log and manifest file
                    // both doesn't exist. This means build is still in process.
                    // http response: 102

                    _logger.LogError(ePending.Message);
                    _logger.LogInformation("Unable to find Build log and Build manifest in the destination");
                    var response = new BuildResponse();
                    response.Message = "Unable to find Build log and Build manifest in the destination";
                    response.Status = BuildState.InProcess.ToString();
                    response.StatusCode = (int)HttpStatusCode.Processing;
                    return StatusCode((int)HttpStatusCode.Processing, response);
                }
                _logger.LogError(ex.Message);
                
            }
            
            return NotFound(output);
        }

        [HttpPost]
        public async Task<IActionResult> Build([FromBody] BuildServerRequests requestData)
        {
            int exitCode = 0;
            string output = string.Empty;
            string error = string.Empty;
            try 
            {
                string jsonString = System.Text.Json.JsonSerializer.Serialize(requestData);
                Console.WriteLine(jsonString);
                _logger.LogInformation($"Request body received: {jsonString}");
                if (requestData == null
                    || requestData.Source == null
                    || requestData.Destination == null
                    || requestData.Platform == null
                    || requestData.PlatformVersion == null)
                {
                    _logger.LogError("Request Body empty or missing Source, destination, platform and platformversion info.");
                    return BadRequest();
                }
                else 
                {
                    
                    _logger.LogDebug("Request Body:", jsonString);
                    var buildScript = new ShellScriptBuilder()
                        .AddCommand(
                        $"oryx build {requestData.Source} -i /tmp/int --platform {requestData.Platform} " +
                        $"--platform-version {requestData.PlatformVersion} -o {requestData.Destination} " +
                        $"--log-file {requestData.LogFile}").ToString();
                    (exitCode, output, error) = await Task.Run(() => RunOryxCommand(buildScript)).ConfigureAwait(false);
                    _logger.LogDebug($"exitcode {exitCode} and output: {output}");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                string msg = "Unable to Process Oryx Build request due to:"+ error;
                return StatusCode((int)HttpStatusCode.InternalServerError, msg);
            }
            
            return StatusCode((int)HttpStatusCode.OK, output);
        }

        private (int exitCode, string output, string error) RunOryxCommand(string script)
        {
            var output = string.Empty;
            var error = string.Empty;
            int exitCode = -1;
            Exception ex = null;

            try
            {
                (exitCode, output, error) = ProcessHelper.RunProcess(
                        "/bin/sh",
                        new[] { "-c", script },
                        workingDirectory: null,
                        waitTimeForExit: null);
            }
            catch (InvalidOperationException invalidOperationException)
            {
                ex = invalidOperationException;
            }
            return (exitCode, output, error); ;
        }
    }
}