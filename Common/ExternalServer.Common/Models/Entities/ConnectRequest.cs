﻿using System;

namespace ExternalServer.Common.Models.Entities {
    public class ConnectRequest {

        public Guid BasestationId { get; set; }

        public bool ForceRelay { get; set; }
    }
}
