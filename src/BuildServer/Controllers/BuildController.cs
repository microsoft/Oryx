// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.Oryx.BuildServer.Controllers
{
    [Route("Build")]
    [ApiController]
    public class BuildController : Controller
    {
        private readonly ILogger<BuildController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BuildController(ILogger<BuildController> logger, IHttpContextAccessor accessor)
        {
            _logger = logger;
            _httpContextAccessor = accessor;
        }

        [Route("")]
        [Route("/Index")]
        [Produces("application/json")]
        [HttpGet]
        public JsonResult Index()
        {
            return Json((int)HttpStatusCode.OK);
        }

        //[Route("[controller]/[action]")]
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
            var response = new BuildServerResponse();
            var httpRequestObject = _httpContextAccessor.HttpContext.Request;
            response.StatusCheckUrl = GetStatusUrls(httpRequestObject);

            try
            {
                (exitCode, output, error) = await Task.Run(() => RunOryxCommand(script)).ConfigureAwait(false);
                response.Message = new BuildOutput(output, error);
                response.Status = BuildState.Running.ToString();
                response.StatusCode = (int)HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                response.Message = new BuildOutput(string.Empty, ex.Message);
                response.Status = BuildState.Failed.ToString();
                response.StatusCode = (int)HttpStatusCode.BadRequest;
            }

            return StatusCode(response.StatusCode, response);
        }

        [Route("/build/CheckBuildStatus")]
        [Produces("application/json")]
        [HttpGet]
        // 
        // GET: /build/CheckBuildStatus/ 
        public async Task<IActionResult> CheckBuildStatus(string manifestfilefullpath, string logfilefullpath)
        {
            int exitCode = 0;
            string output = string.Empty;
            string error = string.Empty;
            var response = new BuildServerResponse();
            var httpRequestObject = _httpContextAccessor.HttpContext.Request;
            response.StatusCheckUrl = GetStatusUrls(httpRequestObject);
            var defaultMsg = "Unknown Build Info";
            response.Message = new BuildOutput(string.Empty, defaultMsg);
            response.Status = BuildState.Unknown.ToString();
            response.StatusCode = (int)HttpStatusCode.Accepted;

            if (string.IsNullOrEmpty(logfilefullpath))
            {
                var msg = "mlogfilefullpath is empty in requestbody.";
                 _logger.LogError(msg);
                response.Message = new BuildOutput(String.Empty, msg); ;
                response.Status = BuildState.Failed.ToString();
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                return StatusCode(response.StatusCode, response); ;
            }

            if (string.IsNullOrEmpty(manifestfilefullpath))
            {
                var msg = "manifestfilefullpath is empty in requestbody.";
                _logger.LogError(msg);
                response.Message = new BuildOutput(String.Empty, msg); ;
                response.Status = BuildState.Failed.ToString();
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                return StatusCode(response.StatusCode, response); ;
            }

            var buildManifestFilePath = manifestfilefullpath;
            var buildLogFilePath = logfilefullpath;

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
                response.Message = new BuildOutput(buildManifestFilePath, error);
                response.Status = BuildState.Success.ToString();
                response.StatusCode = (int)HttpStatusCode.OK;
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                // Checking for a scenario where build log exists but
                // manifest file doesn't. This means it's a failed build scenario
                // http response: 400

                _logger.LogError(ex.Message);
                script = new ShellScriptBuilder()
                .AddFileDoesNotExistCheck(buildManifestFilePath)
                .AddFileExistsCheck(buildLogFilePath)
                .ToString();
                try
                {
                    _logger.LogInformation("Checking if manifestfile doesn't exist but build log exists");
                    (exitCode, output, error) = await Task.Run(() => RunOryxCommand(script)).ConfigureAwait(false);
                    _logger.LogError(output);
                    response.Message = new BuildOutput(output, error);
                    response.Status = BuildState.Failed.ToString();
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return StatusCode(response.StatusCode, response);
                }
                catch (Exception ePending)
                {
                    // Checking for a scenario where build log and manifest file
                    // both doesn't exist. This means build is still in process.
                    // http response: 202
                    _logger.LogError(ePending.Message);
                    var msg = "Unable to find Build log and Build manifest in the destination";
                    _logger.LogInformation(msg);
                    response.Message = new BuildOutput(msg, ePending.Message);
                    response.Status = BuildState.Running.ToString();
                    response.StatusCode = (int)HttpStatusCode.Accepted;
                    return StatusCode(response.StatusCode, response);
                }
            }
        }

        [Route("")]
        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> Build([FromBody] BuildServerRequests requestData)
        {
            int exitCode = 0;
            string output = string.Empty;
            string error = string.Empty;
            var response = new BuildServerResponse();
            var httpRequestObject = _httpContextAccessor.HttpContext.Request;
            response.StatusCheckUrl = GetStatusUrls(httpRequestObject);
            var msg = string.Empty;

            try 
            {
                string jsonString = System.Text.Json.JsonSerializer.Serialize(requestData);
                Console.WriteLine(jsonString);
                _logger.LogInformation($"Request body received: {jsonString}");

                var emptyRequestString = "is empty/null";
                if (requestData == null)
                {
                    msg = $"Request body {emptyRequestString}";
                    _logger.LogError(msg);
                    response.Message = new BuildOutput(string.Empty, msg);
                    response.Status = BuildState.InvalidRequestParameter.ToString();
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return StatusCode(response.StatusCode, response);
                }
                else if (requestData.Source == null)
                {
                    msg = $"Source {emptyRequestString}";
                    _logger.LogError(msg);
                    response.Message = new BuildOutput(string.Empty, msg);
                    response.Status = BuildState.InvalidRequestParameter.ToString();
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return StatusCode(response.StatusCode, response);
                }
                else if (requestData.Destination == null)
                {
                    msg = $"Destination {emptyRequestString}";
                    _logger.LogError(msg);
                    response.Message = new BuildOutput(string.Empty, msg);
                    response.Status = BuildState.InvalidRequestParameter.ToString();
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return StatusCode(response.StatusCode, response);
                }
                else if (requestData.Platform == null)
                {
                    msg = $"Platform {emptyRequestString}";
                    _logger.LogError(msg);
                    response.Message = new BuildOutput(string.Empty, msg);
                    response.Status = BuildState.InvalidRequestParameter.ToString();
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return StatusCode(response.StatusCode, response);
                }
                else if (requestData.PlatformVersion == null)
                {
                    msg = $"PlatformVersion {emptyRequestString}";
                    _logger.LogError(msg);
                    response.Message = new BuildOutput(string.Empty, msg);
                    response.Status = BuildState.InvalidRequestParameter.ToString();
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return StatusCode(response.StatusCode, response);
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
                    response.Status = BuildState.Success.ToString();
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.Message = new BuildOutput(output, error);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                msg = "Unable to Process Oryx Build request...";
                response.Status = BuildState.Failed.ToString();
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = new BuildOutput(msg, e.Message);
                return StatusCode(response.StatusCode, response);
            }

            return StatusCode(response.StatusCode, response);
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

        private StatusUrl GetStatusUrls(HttpRequest request)
        {
            var result = new StatusUrl();
            string host = request.Host.Value;
            string scheme = request.Scheme;
            string buildUrlQueryParam = "?manifestfilefullpath=<value>&logfilefullpath=<value>";
            string buildUrl = string.Concat(scheme, "://", host, "/build/", "CheckBuildStatus", buildUrlQueryParam);
            string serverUrl = string.Concat(scheme, "://", host, "/build/", "CheckServerStatus");

            result.BuildStatusCheckUrl = buildUrl;
            result.ServerStatusCheckUrl = serverUrl;

            return result;
        }
    }
}