using System.Net;
using ExternalServer.Common.Specifications.DataObjects.Dto;

namespace ExternalServer.Common.Models.DTOs {
    public class ConnectRequestResultDto : IConnectRequestResultDto {

        public bool BasestationNotReachable { get; set; }

        public string BasestaionEndPoint { get; set; }
    }
}
