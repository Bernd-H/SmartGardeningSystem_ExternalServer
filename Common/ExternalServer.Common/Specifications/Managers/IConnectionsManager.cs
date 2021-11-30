using System;
using System.Net;
using System.Threading;

namespace ExternalServer.Common.Specifications.Managers {
    public interface IConnectionsManager {

        /// <summary>
        /// Starts accepting connection requests form basestations and hold's them alive.
        /// </summary>
        void Start(CancellationToken token);

        /// <summary>
        /// Notifies the basestation that a user want's to build a connection.
        /// The basestation will try to open a public port via STUN.
        /// If that fails, all traffic will be relayed threw this server.
        /// </summary>
        /// <param name="basestationId"></param>
        /// <returns>The Endpoint the client should send it's request to. NULL if there is no connection to the basestation.</returns>
        IPEndPoint SendUserConnectRequest(Guid basestationId);

        /// <summary>
        /// Removes closed connections from the list.
        /// </summary>
        void CleanupGhostConenctions();
    }
}
