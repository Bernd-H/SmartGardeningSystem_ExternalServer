namespace ExternalServer.Common.Specifications.DataObjects {
    public enum PackageType {
        Init = 0,
        Relay = 1,
        PeerToPeerInit = 2,
        ExternalServerRelayInit = 3,
        Error = 4
    }

    public interface IWanPackage {

        PackageType PackageType { get; set; }

        byte[] Package { get; set; }

        IServiceDetails ServiceDetails { get; set; }
    }
}
