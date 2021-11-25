using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace ExternalServer.RestAPI.Controllers {
    public static class ControllerHelperClass {
        public static string GetUserId(HttpContext httpContext) {
            return httpContext.User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == JwtRegisteredClaimNames.Jti).FirstOrDefault()?.Value;
        }
    }
}
