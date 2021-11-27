using System;
using System.Linq;
using System.Security.Cryptography;
using ExternalServer.Common.Specifications;
using ExternalServer.Common.Specifications.Cryptography;
using NLog;

namespace ExternalServer.BusinessLogic.Cryptography {
    public sealed class PasswordHasher : IPasswordHasher {
        private const int SaltSize = 16; // 128 bit 
        private const int KeySize = 32; // 256 bit

        private ILogger Logger;

        public PasswordHasher(ILoggerService loggerService) {
            Logger = loggerService.GetLogger<PasswordHasher>();
        }

        public bool VerifyHashedPassword(Guid userId, string hashedPassword, byte[] providedPassword) {
            try {
                var parts = hashedPassword.Split('.', 3);

                if (parts.Length != 3) {
                    throw new FormatException("Unexpected hash format. " +
                      "Should be formatted as `{iterations}.{salt}.{hash}`");
                }

                var iterations = Convert.ToInt32(parts[0]);
                var salt = Convert.FromBase64String(parts[1]);
                var key = Convert.FromBase64String(parts[2]);

                using (var algorithm = new Rfc2898DeriveBytes(providedPassword, salt, iterations, HashAlgorithmName.SHA512)) {
                    var keyToCheck = algorithm.GetBytes(KeySize);

                    var verified = keyToCheck.SequenceEqual(key);

                    return verified;
                }
            }
            catch (FormatException ex) {
                Logger.Fatal(ex, $"[VerifyHashedPassword]Wrong stored hash format by user {userId}.");
            }
            catch (Exception ex) {
                Logger.Error(ex, $"[VerifyHashedPassword]Exception while verifying password from user {userId}.");
            }

            return false;
        }
    }
}
