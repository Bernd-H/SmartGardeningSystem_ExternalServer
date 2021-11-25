using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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

        // GET: api/<IPLookupController>
        [HttpGet]
        public IEnumerable<string> Get() {
            //return new string[] { "value1", "value2" };
            throw new NotImplementedException();
        }

        // GET api/<IPLookupController>/5
        [HttpGet("{id}")]
        public ActionResult<IPEndPoint> Get(string basestationId) {
            Logger.Info($"[Get]Endpoint of basestation with id={basestationId} reqeusted from {ControllerHelperClass.GetUserId(HttpContext)}.");
        }
    }
}
