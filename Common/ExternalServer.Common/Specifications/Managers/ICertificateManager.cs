using System.Security.Cryptography.X509Certificates;

namespace ExternalServer.Common.Specifications.Managers {
    public interface ICertificateManager {

        X509Certificate2 GetCertificate();
    }
}
