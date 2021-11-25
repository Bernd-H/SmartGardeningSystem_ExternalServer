using System;

namespace ExternalServer.Common.Specifications.DataObjects {
    public interface IUnmanagedMemoryObject {

        public IntPtr Pointer { get; set; }

        public long Length { get; set; }

        /// <summary>
        /// Obfuscates and frees memory.
        /// </summary>
        public void ClearMemory();
    }
}
