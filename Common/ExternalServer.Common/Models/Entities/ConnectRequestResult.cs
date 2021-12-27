using System.Net;
using System.Net.Security;
using ExternalServer.Common.Specifications.DataObjects;

namespace ExternalServer.Common.Models.Entities {
    public class ConnectRequestResult : IConnectRequestResult {

        public IPEndPoint PeerToPeerEndPoint { get; set; }

        public SslStream basestationStream { get; set; }
    }
}
