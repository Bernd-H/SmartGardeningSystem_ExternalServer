using System.Net;

namespace ExternalServer.Common.Specifications.DataObjects.Dto {
    public interface IConnectRequestResultDto {

        bool BasestationNotReachable { get; set; }

        string BasestaionEndPoint { get; set; }
    }
}
