using System;
using System.Threading;
using ExternalServer.Common.Models.Entities;
using ExternalServer.Common.Specifications.DataObjects;
using ExternalServer.Common.Specifications.Jobs;

namespace ExternalServer.Common.Specifications.Managers {

    /// <summary>
    /// Manager that handles connections to basestations that are made for exchanging information about a
    /// connection establishment of a mobile app.
    /// </summary>
    public interface IConnectionsManager {

        /// <summary>
        /// Starts accepting connection requests form basestations and hold's them alive.
        /// </summary>
        /// <param name="token">Cancellation token to stop this service.</param>
        void Start(CancellationToken token);

        /// <summary>
        /// Notifies the basestation that a user want's to establish a connection.
        /// The basestation will try to open a public port via STUN.
        /// If that fails, all traffic will be relayed threw another connection to this server. (-> RelayManager)
        /// </summary>
        /// <param name="connectRequest">Information about the connect request of the mobile app.</param>
        /// <returns>Containes if the baseastion was reachable and the Endpoint of the basestation if peer to peer is possible.</returns>
        /// <seealso cref="IRelayManager"/>
        IConnectRequestResult SendUserConnectRequest(ConnectRequest connectRequest);

        /// <summary>
        /// Removes closed connections from the list.
        /// Gets called frequently by the ConnectorJob.
        /// </summary>
        /// <seealso cref="IConnectorJob"/>
        void CleanupGhostConenctions();
    }
}
