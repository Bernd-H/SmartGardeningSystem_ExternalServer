using System.Net;

namespace ExternalServer.Common.Specifications.DataObjects.Dto {
    public interface IRelayRequestResultDto {

        bool BasestationNotReachable { get; set; }

        IPEndPoint BasestaionEndPoint { get; set; }
    }
}
