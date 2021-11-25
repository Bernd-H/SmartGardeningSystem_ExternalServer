using System;
using ExternalServer.Common.Specifications.DataObjects;

namespace ExternalServer.Common.Models.DTOs {
    public class UserDto : IDisposable {

        public IUnmanagedMemoryObject Email { get; set; }

        public IUnmanagedMemoryObject HashedPassword { get; set; }

        public UserDto() {

        }

        public void Dispose() {
            Email.ClearMemory();
            HashedPassword.ClearMemory();
        }
    }
}
