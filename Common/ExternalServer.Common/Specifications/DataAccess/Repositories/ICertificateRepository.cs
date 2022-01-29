using System.Security.Cryptography.X509Certificates;

namespace ExternalServer.Common.Specifications.DataAccess.Repositories {

    /// <summary>
    /// Repository that loads certificates from the hard drive or the internal cache.
    /// </summary>
    public interface ICertificateRepository {

        /// <summary>
        /// Gets a certificate from X509Store or from the internal cache.
        /// Reloads a cached certificate after 5 days.
        /// </summary>
        /// <param name="filePath">File path where the certificate is stored.</param>
        /// <returns>A X509 certificate that contains also it's private key.</returns>
        X509Certificate2 GetCertificate(string filePath);
    }
}
