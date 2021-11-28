using System;
using System.Linq;
using System.Threading.Tasks;
using ExternalServer.Common.Models.Entities;
using ExternalServer.Common.Specifications;
using ExternalServer.Common.Specifications.DataAccess.Repositories;
using ExternalServer.DataAccess.Database;
using NLog;

namespace ExternalServer.DataAccess.Repositories {
    public class BasestationIpRepository : DbBaseRepository<BasestationIP>, IBasestationIpRepository {

        private ILogger Logger;

        public BasestationIpRepository(ILoggerService loggerService) : base(loggerService) {
            Logger = loggerService.GetLogger<BasestationIpRepository>();
        }

        public async Task<bool> AddBasestation(BasestationIP bip) {
            return await AddToTable(bip) == 1;
        }

        public async Task<bool> RemoveBasestation(Guid id) {
            var bip = await QueryById(id);
            if (bip != null) {
                return await RemoveFromTable(bip) == 1;
            }

            return false;
        }

        public async Task<bool> UpdateOrAddBasestation(BasestationIP bip) {
            if (await QueryById(bip.Id) != null) {
                return await UpdateObject(bip);
            }
            else {
                return await AddBasestation(bip);
            }
        }

        public async Task<BasestationIP> QueryById(Guid Id) {
            await LOCKER.WaitAsync();

            var bip = context.BasestationIPs.Where(u => u.Id == Id).FirstOrDefault();

            LOCKER.Release();
            return bip;
        }
    }
}
