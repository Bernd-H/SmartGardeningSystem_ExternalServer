using System;
using System.Net.Security;
using System.Threading;

namespace ExternalServer.Common.Specifications.Managers {

    /// <summary>
    /// Administrates relay connections to basestations.
    /// </summary>
    public interface IRelayManager {

        void Start(CancellationToken cancellationToken);

        SslStream GetTunnelToBasestation(Guid tunnelId);
    }
}
