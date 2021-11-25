using ExternalServer.Common.Models;
using ExternalServer.Common.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExternalServer.RestAPI.Controllers.MobileAppControllers {
    [Route("api/[controller]")]
    [ApiController]
    public class ClientRegisterController : ControllerBase {
        // POST api/<RegisterController>
        [HttpPost]
        [AllowAnonymous]
        public IActionResult Post([FromBody] User registerInformation) {
            // convert confidential data to secure memory objects
            var registerInfoDto = registerInformation.ToDto();

            // check if email is free

            // confirm email

            // store data in database

            return Ok();
        }
    }
}
