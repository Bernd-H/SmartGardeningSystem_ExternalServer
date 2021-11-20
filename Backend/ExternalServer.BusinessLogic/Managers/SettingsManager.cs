using System;
using ExternalServer.Common.Models.DTOs;
using ExternalServer.Common.Specifications.Managers;

namespace ExternalServer.BusinessLogic.Managers {
    public class SettingsManager : ISettingsManager {

        public SettingsManager() {

        }

        public void DeleteSettings() {
            throw new NotImplementedException();
        }

        public ApplicationSettingsDto GetApplicationSettings(ICertificateManager CertificateManager = null) {
            throw new NotImplementedException();
        }

        public void UpdateCurrentSettings(Func<ApplicationSettingsDto, ApplicationSettingsDto> updateFunc, ICertificateManager CertificateManager = null) {
            throw new NotImplementedException();
        }
    }
}
