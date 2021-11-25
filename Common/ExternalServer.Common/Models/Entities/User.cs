using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExternalServer.Common.Specifications.DataObjects;
using ExternalServer.Common.Utilities;

namespace ExternalServer.Common.Models.Entities {
    public class User : IEFModel, IDisposable {

        [Key]
        public Guid Id { get; set; }

        [Required]
        [Column(TypeName = "BLOB")]
        public byte[] Email { get; set; }

        [Required]
        [Column(TypeName = "BLOB")]
        public byte[] HashedPassword { get; set; }

        public User() {

        }

        public void Dispose() {
            CryptoUtils.ObfuscateByteArray(Email);
            CryptoUtils.ObfuscateByteArray(HashedPassword);
        }
    }
}
