using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ExternalServer.Common.Configuration;
using ExternalServer.Common.Models;
using ExternalServer.Common.Models.DTOs;
using ExternalServer.Common.Models.Entities;
using ExternalServer.Common.Specifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NLog;

namespace ExternalServer.RestAPI.Controllers.MobileAppControllers {
    [Route("api/[controller]")]
    [ApiController]
    public class ClientLoginController : ControllerBase {

        private IConfiguration Configuration;

        private ILogger Logger;

        public ClientLoginController(ILoggerService loggerService, IConfiguration configuration) {
            Logger = loggerService.GetLogger<ClientLoginController>();
            Configuration = configuration;
        }

        // POST api/<ClientLoginController>
        [HttpPost]
        public IActionResult Login([FromBody] User loginInfo) {
            // convert confidential data to secure memory objects
            var loginInfoDto = loginInfo?.ToDto();

            IActionResult response = Unauthorized();

            if (loginInfo != null) {
                // check if database entry is the same as the provided data
                if (AuthenticateUser(loginInfoDto)) {
                    response = generateJSONWebTokenForMobileApp(loginInfoDto);
                }
            }

            return response;
        }

        /// <summary>
        /// Creates a json web token of type "mobileapp" with a lifetime of 120 minutes.
        /// </summary>
        private IActionResult generateJSONWebTokenForMobileApp(UserDto loginInfo) {
            Logger.Info($"[generateJSONWebTokenForMobileApp]Generating json web token for mobile app.");

            try {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(""));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new[] {
                    new Claim(JwtRegisteredClaimNames.Typ, "mobileapp"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var token = new JwtSecurityToken(Configuration[ConfigurationVars.JWT_ISSUER],
                        Configuration[ConfigurationVars.JWT_ISSUER],
                        claims,
                        expires: DateTime.Now.AddMinutes(120),
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
        private bool AuthenticateUser(UserDto loginInfo) {
            return true;
        }
    }
}
