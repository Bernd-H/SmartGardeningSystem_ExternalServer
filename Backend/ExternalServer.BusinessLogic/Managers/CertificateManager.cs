using System.Security.Cryptography.X509Certificates;
using ExternalServer.Common.Configuration;
using ExternalServer.Common.Specifications;
using ExternalServer.Common.Specifications.DataAccess.Repositories;
using ExternalServer.Common.Specifications.Managers;
using Microsoft.Extensions.Configuration;
using NLog;

namespace ExternalServer.BusinessLogic.Managers {
    public class CertificateManager : ICertificateManager {

        private ILogger Logger;

        internal ICertificateRepository CertificateRepository;

        internal IConfiguration Configuration;

        public CertificateManager(ILoggerService loggerService, ICertificateRepository certificateRepository, IConfiguration configuration) {
            Logger = loggerService.GetLogger<CertificateManager>();
            CertificateRepository = certificateRepository;
            Configuration = configuration;
        }

        public X509Certificate2 GetCertificate() {
            return CertificateRepository.GetCertificate(Configuration[ConfigurationVars.CERTIFICATE_FILEPATH]);
        }
    }
}
