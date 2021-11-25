using System.Threading.Tasks;
using ExternalServer.Common.Models.Entities;

namespace ExternalServer.Common.Specifications.DataAccess.Repositories {
    public interface IUserRepository {

        Task<bool> AddUser(User user);

        Task<bool> RemoveUser(User user);

        Task<User> QueryByEmail(byte[] email);
    }
}
