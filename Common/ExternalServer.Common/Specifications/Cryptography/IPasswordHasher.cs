using System;

namespace ExternalServer.Common.Specifications.Cryptography {

    /// <summary>
    /// Class to hash and verify passwords.
    /// </summary>
    public interface IPasswordHasher {

        /// <summary>
        /// Verifies if both passwords are the same
        /// </summary>
        /// <param name="hashedPassword">Hashed password string.</param>
        /// <param name="providedPassword">Password in plaintext.</param>
        /// <param name="user">for logging purposes</param>
        /// <returns>True when the <paramref name="hashedPassword"/> and the hash of <paramref name="providedPassword"/> are the same.</returns>
        bool VerifyHashedPassword(Guid userId, string hashedPassword, byte[] providedPassword);
    }
}
