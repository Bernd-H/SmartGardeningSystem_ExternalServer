using System;
using ExternalServer.Common.Utilities;

namespace ExternalServer.Common.Models.Entities {
    public class User : IDisposable {
        public byte[] Email { get; set; }

        public byte[] HashedPassword { get; set; }

        public User() {

        }

        public void Dispose() {
            CryptoUtils.ObfuscateByteArray(Email);
            CryptoUtils.ObfuscateByteArray(HashedPassword);
        }
    }
}
