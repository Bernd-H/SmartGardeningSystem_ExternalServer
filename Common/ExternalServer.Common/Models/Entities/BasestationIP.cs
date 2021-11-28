using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExternalServer.Common.Specifications.DataObjects;

namespace ExternalServer.Common.Models.Entities {
    public class BasestationIP : IEFModel {

        /// <summary>
        /// Basestation IP
        /// </summary>
        [Key]
        [Required]
        public Guid Id { get; set; }

        /// <summary>
        /// Public IP of basestation
        /// </summary>
        [Required]
        [Column(TypeName = "VARCHAR(15)")]
        public string Ip { get; set; }
    }
}
