using System.Security.Cryptography.X509Certificates;

namespace ExternalServer.Common.Specifications.Managers {

    /// <summary>
    /// Administrates the server certificate.
    /// </summary>
    public interface ICertificateManager {

        /// <summary>
        /// Gets the server certificate.
        /// </summary>
        /// <returns>A X509Certificate containing the private key.</returns>
        X509Certificate2 GetCertificate();
    }
}
