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
        /// Null if basestation could not be reached.
        /// </summary>
        SslStream basestationStream { get; set; }
    }
}
