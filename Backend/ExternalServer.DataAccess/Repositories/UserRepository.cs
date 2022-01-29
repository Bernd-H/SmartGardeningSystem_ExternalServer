using System;
using System.Linq;
using System.Threading.Tasks;
using ExternalServer.Common.Models.Entities;
using ExternalServer.Common.Specifications;
using ExternalServer.Common.Specifications.DataAccess.Repositories;
using ExternalServer.DataAccess.Database;
using NLog;

namespace ExternalServer.DataAccess.Repositories {

    /// <inheritdoc/>
    public class UserRepository : DbBaseRepository<User>, IUserRepository {

        private ILogger Logger;

        public UserRepository(ILoggerService loggerService) : base(loggerService) {
            Logger = loggerService.GetLogger<UserRepository>();
        }

        /// <inheritdoc/>
        public async Task<bool> AddUser(User user) {
            return await AddToTable(user) == 1;
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveUser(User user) {
            return await RemoveFromTable(user) == 1;
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateUser(User updatedUser) {
            return await UpdateObject(updatedUser);
        }

        /// <inheritdoc/>
        public async Task<User> QueryByEmail(byte[] email) {
            await LOCKER.WaitAsync();

            var user = context.users.Where(u => u.Email == email).FirstOrDefault();

            LOCKER.Release();
            return user;
        }

        /// <inheritdoc/>
        public async Task<User> QueryById(Guid Id) {
            await LOCKER.WaitAsync();

            var user = context.users.Where(u => u.Id == Id).FirstOrDefault();

            LOCKER.Release();
            return user;
        }
    }
}
