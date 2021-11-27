using System;
using System.Net;
using ExternalServer.Common.Specifications;
using ExternalServer.Common.Specifications.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace ExternalServer.RestAPI.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class IPLookupController : ControllerBase {

        private IConnectionsManager ConnectionsManager;

        private ILogger Logger;

        public IPLookupController(ILoggerService loggerService, IConnectionsManager connectionsManager) {
            Logger = loggerService.GetLogger<IPLookupController>();
            ConnectionsManager = connectionsManager;
        }

        // GET api/<IPLookupController>/5
        [HttpGet("{id}")]
        public ActionResult<IPEndPoint> Get(string basestationId) {
            if (ControllerHelperClass.CallerIsUser(HttpContext)) {
                Logger.Info($"[Get]Endpoint of basestation with id={basestationId} reqeusted from {ControllerHelperClass.GetUserId(HttpContext)}.");
                throw new NotImplementedException();
            }
            else {
                return Unauthorized();
            }
        }

        // POST api/<IPLookupController>
        [HttpPost]
        public IActionResult Post([FromBody] string basestationId) {
            if (ControllerHelperClass.CallerIsBasestation(HttpContext)) {
                Logger.Info($"[Get]Endpoint of basestation with id={basestationId} reqeusted from {ControllerHelperClass.GetUserId(HttpContext)}.");
                throw new NotImplementedException();
            }
            else {
                return Unauthorized();
            }
        }
    }
}
