using ExternalServer.Common.Specifications.DataObjects;

namespace ExternalServer.Common.Models.Entities {
    public class WanPackage : IWanPackage {

        public PackageType PackageType { get; set; }

        public byte[] Package { get; set; }

        public IServiceDetails ServiceDetails { get; set; }
    }
}
