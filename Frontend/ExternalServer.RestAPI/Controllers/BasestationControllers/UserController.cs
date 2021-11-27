using System;
using ExternalServer.Common.Models.DTOs;
using ExternalServer.Common.Models.Entities;
using ExternalServer.Common.Specifications;
using ExternalServer.Common.Specifications.Cryptography;
using ExternalServer.Common.Specifications.DataAccess.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace ExternalServer.RestAPI.Controllers.BasestationControllers {
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase {

        private ILogger Logger;

        private IUserRepository UserRepository;

        private IPasswordHasher PasswordHasher;

        public UserController(ILoggerService loggerService, IUserRepository userRepository, IPasswordHasher passwordHasher) {
            Logger = loggerService.GetLogger<UserController>();
            UserRepository = userRepository;
            PasswordHasher = passwordHasher;
        }

        // GET api/<UserController>/<emailAddress>
        [HttpGet("{email}")]
        public ActionResult<User> GetUserDetails(string email) {
            if (ControllerHelperClass.CallerIsBasestation(HttpContext)) {
                if (string.IsNullOrEmpty(email)) {
                    return BadRequest();
                }

                Logger.Info($"[GetUserDetails]User detailes requested.");
                return UserRepository.QueryByEmail(Convert.FromBase64String(email)).Result;
            }

            return Unauthorized();
        }

        // POST api/<UserController>
        [HttpPost]
        public IActionResult AddUser([FromBody] User registerInformation) {
            if (ControllerHelperClass.CallerIsBasestation(HttpContext)) {
                Logger.Info($"[Post]Registering a new user.");

                // check if email is free
                if (UserRepository.QueryByEmail(registerInformation?.Email).Result == null) {
                    Logger.Debug($"[Post]Adding user with email {registerInformation.Email}.");
                    // TODO: confirm email

                    // store data in database
                    UserRepository.AddUser(registerInformation);

                    return Ok();
                }

                return Problem();
            }
            else {
                return Unauthorized();
            }
        }

        // PUT api/<UserController>/5
        [HttpPut]
        public IActionResult UpdateUserHash([FromBody] ChangeUserInfoDto userInfo) {
            if (ControllerHelperClass.CallerIsBasestation(HttpContext)) {
                Logger.Info($"[UpdateUserHash]In update user hash.");

                var storedUser = UserRepository.QueryById(userInfo.Id).Result;
                if (storedUser != null) {
                    // check if plaintext password is the one password which is stored in the database
                    // hash password
                    var verified = PasswordHasher.VerifyHashedPassword(userInfo.Id, storedUser.HashedPassword, userInfo.PlainTextPassword);
                    if (verified) {
                        // udpate database
                        UserRepository.UpdateUser(new User {
                            Id = userInfo.Id,
                            Email = storedUser.Email,
                            HashedPassword = userInfo.NewPasswordHash
                        });

                        return Ok();
                    }
                    else {
                        return Unauthorized();
                    }
                }

                return BadRequest();
            }
            else {
                return Unauthorized();
            }
        }

        // DELETE api/<UserController>/5
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(Guid id) {
            if (ControllerHelperClass.CallerIsBasestation(HttpContext)) {
                throw new NotImplementedException();
            }
            else {
                return Unauthorized();
            }
        }
    }
}
