using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExternalServer.RestAPI.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : ControllerBase {
        // POST api/<RegisterController>
        [HttpPost]
        [AllowAnonymous]
        public void Post([FromBody] string value) {

        }

        // PUT api/<RegisterController>/5
        [HttpPut("{id}")]
        [Authorize]
        public void Put(int id, [FromBody] string value) {

        }

        // DELETE api/<RegisterController>/5
        [HttpDelete("{id}")]
        [Authorize]
        public void Delete(int id, [FromBody] string value) {

        }
    }
}
