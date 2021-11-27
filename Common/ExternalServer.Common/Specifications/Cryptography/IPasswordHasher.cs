using System;

namespace ExternalServer.Common.Specifications.Cryptography {
    public interface IPasswordHasher {

        /// <summary>
        /// Verifies if both passwords are the same
        /// </summary>
        /// <param name="hashedPassword"></param>
        /// <param name="providedPassword"></param>
        /// <param name="user">for logging purposes</param>
        /// <returns>if verified or not</returns>
        bool VerifyHashedPassword(Guid userId, string hashedPassword, byte[] providedPassword);
    }
}
