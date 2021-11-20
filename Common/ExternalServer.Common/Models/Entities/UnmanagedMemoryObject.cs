using System;
using ExternalServer.Common.Specifications.DataObjects;

namespace ExternalServer.Common.Models.Entities {
    public class UnmanagedMemoryObject : IUnmanagedMemoryObject {

        public IntPtr Pointer { get; set; }

        public long Length { get; set; }

        public UnmanagedMemoryObject() {

        }
    }
}
