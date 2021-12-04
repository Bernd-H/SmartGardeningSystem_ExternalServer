using System;
using System.Net;
using ExternalServer.Common.Models.DTOs;
using ExternalServer.Common.Specifications;
using ExternalServer.Common.Specifications.DataAccess.Repositories;
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

        private IBasestationIpRepository BasestationIpRepository;

        private ILogger Logger;

        public IPLookupController(ILoggerService loggerService, IConnectionsManager connectionsManager, IBasestationIpRepository basestationIpRepository) {
            Logger = loggerService.GetLogger<IPLookupController>();
            ConnectionsManager = connectionsManager;
            BasestationIpRepository = basestationIpRepository;
        }

        //// GET api/<IPLookupController>/5
        //[HttpGet("{id}")]
        //public ActionResult<IPEndPoint> Get(string basestationId) {
        //    if (ControllerHelperClass.CallerIsUser(HttpContext)) {
        //        Logger.Info($"[Get]Endpoint of basestation with id={basestationId} reqeusted from {ControllerHelperClass.GetUserId(HttpContext)}.");
        //        throw new NotImplementedException();
        //    }
        //    else {
        //        return Unauthorized();
        //    }
        //}

        //// POST api/<IPLookupController>
        //[HttpPost]
        //public IActionResult UpdateBasestationIP([FromBody] IPStatusDto ipStatus) {
        //    if (ControllerHelperClass.CallerIsBasestation(HttpContext)) {
        //        string id = ControllerHelperClass.GetUserId(HttpContext);
        //        Logger.Info($"[UpdateBasestationIP]Updating ip from basestation with id={id}.");
        //        if (ipStatus.Id != Guid.Parse(id)) {
        //            Logger.Error($"[UpdateBasestationIP]Wrong basestation id.");
        //            return Problem("Wrong basestation id.");
        //        }

        //        // update ip in database
        //        var success = BasestationIpRepository.UpdateOrAddBasestation(new Common.Models.Entities.BasestationIP {
        //            Id = ipStatus.Id,
        //            Ip = ipStatus.Ip
        //        }).Result;

        //        if (success) {
        //            return Ok();
        //        }

        //        return Problem();
        //    }
        //    else {
        //        return Unauthorized();
        //    }
        //}
    }
}
