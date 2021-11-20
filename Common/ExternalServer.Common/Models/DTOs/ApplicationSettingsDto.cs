using ExternalServer.Common.Specifications.DataObjects;

namespace ExternalServer.Common.Models.DTOs {
    public class ApplicationSettingsDto {

        public IUnmanagedMemoryObject API_JwT_Key { get; set; }

        public ApplicationSettingsDto() {

        }
    }
}
