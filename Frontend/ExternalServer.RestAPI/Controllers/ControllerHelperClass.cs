using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using ExternalServer.Common.Utilities;
using Microsoft.AspNetCore.Http;

namespace ExternalServer.RestAPI.Controllers {
    public static class ControllerHelperClass {
        public static string GetUserId(HttpContext httpContext) {
            return httpContext.User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == JwtClaimTypes.USER_ID).FirstOrDefault()?.Value;
        }

        public static bool CallerIsBasestation(HttpContext httpContext) {
            var type = httpContext.User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == JwtClaimTypes.TYPE).FirstOrDefault().Value;
            
            if (type == "basestation") {
                return true;
            }

            return false;
        }

        public static bool CallerIsUser(HttpContext httpContext) {
            var type = httpContext.User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == JwtClaimTypes.TYPE).FirstOrDefault().Value;

            if (type == "mobileapp") {
                return true;
            }

            return false;
        }
    }
}
