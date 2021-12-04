using ExternalServer.Common.Models.DTOs;
using ExternalServer.Common.Models.Entities;
using ExternalServer.Common.Specifications.DataObjects;
using ExternalServer.Common.Specifications.DataObjects.Dto;
using ExternalServer.Common.Utilities;

namespace ExternalServer.Common.Models {
    public static class DtoConversion {

        public static IRelayRequestResultDto ToDto(this IRelayRequestResult result) {
            return new RelayRequestResultDto {
                BasestationNotReachable = (result.basestationStream == null),
                BasestaionEndPoint = result.PeerToPeerEndPoint
            };
        }

    }
}
