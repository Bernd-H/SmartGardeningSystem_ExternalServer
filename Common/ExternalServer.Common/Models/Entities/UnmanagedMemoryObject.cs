using System;
using ExternalServer.Common.Specifications.DataObjects;
using ExternalServer.Common.Utilities;

namespace ExternalServer.Common.Models.Entities {
    public class UnmanagedMemoryObject : IUnmanagedMemoryObject {

        public IntPtr Pointer { get; set; }

        public long Length { get; set; }

        public UnmanagedMemoryObject() {

        }

        public void ClearMemory() {
            CryptoUtils.ObfuscateAndFreeMemory(Pointer, Length);
            Length = 0;
            Pointer = IntPtr.Zero;
        }
    }
}
