using System;

namespace ExternalServer.Common.Models.DTOs {
    public class IPStatusDto {

        /// <summary>
        /// Basestation ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Public IP addres of basestation
        /// </summary>
        public string Ip { get; set; }
    }
}
