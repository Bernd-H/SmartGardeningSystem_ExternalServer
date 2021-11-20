using System;

namespace ExternalServer.Common.Specifications.DataObjects {
    public interface IUnmanagedMemoryObject {

        public IntPtr Pointer { get; set; }

        public long Length { get; set; }
    }
}
