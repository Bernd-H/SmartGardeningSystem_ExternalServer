﻿using ExternalServer.Common.Specifications.DataObjects.Dto;

namespace ExternalServer.Common.Models.DTOs {
    public class ConnectRequestDto : IConnectRequestDto {

        public byte[] BasestationId { get; set; }

        public bool ForceRelay { get; set; }
    }
}
