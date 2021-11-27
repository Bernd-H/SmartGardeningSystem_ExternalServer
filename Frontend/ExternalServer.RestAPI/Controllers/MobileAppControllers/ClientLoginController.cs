using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ExternalServer.Common.Configuration;
using ExternalServer.Common.Models.Entities;
using ExternalServer.Common.Specifications;
using ExternalServer.Common.Specifications.DataAccess.Repositories;
using ExternalServer.Common.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NLog;

namespace ExternalServer.RestAPI.Controllers.MobileAppControllers {
    [Route("api/[controller]")]
    [ApiController]
    public class ClientLoginController : ControllerBase {

        private IUserRepository UserRepository;

        private IConfiguration Configuration;

        private ILogger Logger;

        public ClientLoginController(ILoggerService loggerService, IConfiguration configuration, IUserRepository userRepository) {
            Logger = loggerService.GetLogger<ClientLoginController>();
            Configuration = configuration;
            UserRepository = userRepository;
        }

        // POST api/<ClientLoginController>
        [HttpPost]
        public IActionResult Login([FromBody] User loginInfo) {
            IActionResult response = Unauthorized();

            if (loginInfo != null) {
                // check if database entry is the same as the provided data
                if (AuthenticateUser(loginInfo).Result) {
                    response = generateJSONWebTokenForMobileApp(loginInfo);
                }
            }

            return response;
        }

        /// <summary>
        /// Creates a json web token of type "mobileapp" with a lifetime of 120 minutes.
        /// </summary>
        private IActionResult generateJSONWebTokenForMobileApp(User loginInfo) {
            Logger.Info($"[generateJSONWebTokenForMobileApp]Generating json web token for mobile app.");

            try {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(""));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new[] {
                    new Claim(JwtClaimTypes.TYPE, "mobileapp"),
                    new Claim(JwtClaimTypes.USER_ID, Guid.NewGuid().ToString())
                };

                var token = new JwtSecurityToken(Configuration[ConfigurationVars.JWT_ISSUER],
                        Configuration[ConfigurationVars.JWT_ISSUER],
                        claims,
                        expires: DateTime.UtcNow.AddMinutes(120),
                        signingCredentials: credentials);

                string tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                return Ok(new { token = tokenString });
            }
            catch (Exception ex) {
                Logger.Fatal(ex, $"[generateJSONWebTokenForMobileApp]An error occured.");
                return base.Problem("Error by creating a json web token.");
            }
        }

        /// <summary>
        /// Checks if login credentials are correct.
        /// </summary>
        private async Task<bool> AuthenticateUser(User loginInfo) {
            var storedUserInfo = await UserRepository.QueryByEmail(loginInfo.Email);
            if (storedUserInfo != null) {
                // compare hashed password
                if (storedUserInfo.HashedPassword.SequenceEqual(loginInfo.HashedPassword)) {
                    return true;
                }
            }

            return false;
        }
    }
}
