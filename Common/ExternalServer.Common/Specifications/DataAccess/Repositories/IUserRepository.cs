using System;
using System.Threading.Tasks;
using ExternalServer.Common.Models.Entities;

namespace ExternalServer.Common.Specifications.DataAccess.Repositories {

    /// <summary>
    /// Administrates user entries in a database.
    /// This repository gets currently not used anywhere.
    /// </summary>
    public interface IUserRepository {

        Task<bool> AddUser(User user);

        Task<bool> RemoveUser(User user);

        Task<bool> UpdateUser(User updatedUser);

        Task<User> QueryByEmail(byte[] email);

        Task<User> QueryById(Guid Id);
    }
}
