using System;
using ExternalServer.Common.Models.DTOs;
using ExternalServer.Common.Models.Entities;
using ExternalServer.Common.Specifications.DataObjects;
using ExternalServer.Common.Specifications.DataObjects.Dto;
using ExternalServer.Common.Utilities;

namespace ExternalServer.Common.Models {
    public static class DtoConversion {

        public static IConnectRequestResultDto ToDto(this IConnectRequestResult result) {
            return new ConnectRequestResultDto {
                BasestationNotReachable = (result.basestationStream == null),
                BasestaionEndPoint = result.PeerToPeerEndPoint?.ToString() ?? string.Empty
            };
        }

        public static ConnectRequest FromDto(this IConnectRequestDto connectRequestDto) {
            return new ConnectRequest {
                BasestationId = new Guid(connectRequestDto.BasestationId),
                ForceRelay = connectRequestDto.ForceRelay
            };
        }

        public static ConnectRequestDto ToDto(this ConnectRequest connectRequest) {
            return new ConnectRequestDto {
                BasestationId = connectRequest.BasestationId.ToByteArray(),
                ForceRelay = connectRequest.ForceRelay
            };
        }
    }
}
