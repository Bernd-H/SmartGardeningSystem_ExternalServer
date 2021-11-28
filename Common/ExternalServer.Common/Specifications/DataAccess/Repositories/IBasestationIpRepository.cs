using System;
using System.Threading.Tasks;
using ExternalServer.Common.Models.Entities;

namespace ExternalServer.Common.Specifications.DataAccess.Repositories {
    public interface IBasestationIpRepository {

        Task<bool> AddBasestation(BasestationIP bip);

        Task<bool> RemoveBasestation(Guid id);

        Task<bool> UpdateOrAddBasestation(BasestationIP bip);

        Task<BasestationIP> QueryById(Guid Id);
    }
}
