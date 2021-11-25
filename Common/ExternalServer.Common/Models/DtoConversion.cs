using ExternalServer.Common.Models.DTOs;
using ExternalServer.Common.Models.Entities;
using ExternalServer.Common.Utilities;

namespace ExternalServer.Common.Models {
    public static class DtoConversion {

        public static UserDto ToDto(this User user) {
            return new UserDto {
                Email = CryptoUtils.MoveDataToUnmanagedMemory(user.Email),
                HashedPassword = CryptoUtils.MoveDataToUnmanagedMemory(user.HashedPassword)
            };
        }

    }
}
