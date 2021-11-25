using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using ExternalServer.Common.Configuration;
using ExternalServer.Common.Specifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NLog;

namespace ExternalServer.RestAPI.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase {

        private IConfiguration Configuration;

        private ILogger Logger;

        public TokenController(IConfiguration configuration, ILoggerService loggerService) {
            Logger = loggerService.GetLogger<TokenController>();
            Configuration = configuration;
        }

        // GET: api/<TokenController>
        [HttpGet]
        public IActionResult Get([FromBody] string guid) {
            try {
                if (isPrivate(Request.Host.Host)) {
                    return generateJSONWebToken(Guid.Parse(guid));
                }
            } catch (Exception ex) {
                Logger.Error(ex, $"[Get]Error while generating json web token for basestation with id={guid}.");
            }

            return Unauthorized();
        }

        private IActionResult generateJSONWebToken(Guid basestationId) {
            Logger.Info($"[GenerateJSONWebToken]Generating a json web token.");

            try {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("TestKey1asdfasdfasdfe3"));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new[] {
                    new Claim(JwtRegisteredClaimNames.Typ, "basestation"),
                    new Claim(JwtRegisteredClaimNames.Jti, basestationId.ToString())
                };

                var token = new JwtSecurityToken(Configuration[ConfigurationVars.JWT_ISSUER],
                        Configuration[ConfigurationVars.JWT_ISSUER],
                        claims,
                        expires: null, // token doesn't expire
                        signingCredentials: credentials);

                string tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                return Ok(new { token = tokenString });
            }
            catch (Exception ex) {
                Logger.Fatal(ex, $"[GenerateJSONWebToken]An Error occured while creating a json web token.");
                return base.Problem("An Error occured while creating a json web token.");
            }
        }

        private bool isPrivate(string ipAddress) {
            if (string.IsNullOrWhiteSpace(ipAddress)) {
                return false;
            }
            if (ipAddress == "localhost") {
                return true;
            }
            
            try {
                int[] ipParts = ipAddress.Split(new String[] { "." }, StringSplitOptions.RemoveEmptyEntries)
                         .Select(s => int.Parse(s)).ToArray();
                // in private ip range
                if (ipParts[0] == 10 ||
                    (ipParts[0] == 192 && ipParts[1] == 168) ||
                    (ipParts[0] == 172 && (ipParts[1] >= 16 && ipParts[1] <= 31))) {
                    return true;
                }
            } catch (Exception) { }

            // IP Address is probably public.
            // This doesn't catch some VPN ranges like OpenVPN and Hamachi.
            return false;
        }
    }
}
