using System.Net;
using ExternalServer.Common.Specifications.DataObjects.Dto;

namespace ExternalServer.Common.Models.DTOs {
    public class RelayRequestResultDto : IRelayRequestResultDto {

        public bool BasestationNotReachable { get; set; }

        public IPEndPoint BasestaionEndPoint { get; set; }
    }
}
