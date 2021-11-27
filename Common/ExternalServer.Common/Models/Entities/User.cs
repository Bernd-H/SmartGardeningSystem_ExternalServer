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

        /// <summary>
        /// Max size = 100 characters
        /// A Hash size of 64 bytes + a salt of 16 bytes and a iteration integer
        /// with 5 digits would need 85 characters to store.
        /// </summary>
        [Required]
        [Column(TypeName = "VARCHAR(100)")]
        public string HashedPassword { get; set; }

        public User() {

        }

        public void Dispose() {
            CryptoUtils.ObfuscateByteArray(Email);
        }
    }
}
