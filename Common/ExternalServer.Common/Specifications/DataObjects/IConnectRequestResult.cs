using System;
using System.Net;
using System.Net.Security;

namespace ExternalServer.Common.Specifications.DataObjects {
    public interface IConnectRequestResult {

        /// <summary>
        /// Public endpoint of the basestation.
        /// Null if peer to peer is not possible.
        /// </summary>
        IPEndPoint PeerToPeerEndPoint { get; set; }


        /// <summary>
        /// Id of an already open connection from the basestation to the external server,
        /// that can be used to relay packages from a mobile app to the basestation.
        /// </summary>
        public Guid TunnelId { get; set; }


        /// <summary>
        /// Null if basestation could not be reached.
        /// </summary>
        SslStream basestationStream { get; set; }
    }
}
