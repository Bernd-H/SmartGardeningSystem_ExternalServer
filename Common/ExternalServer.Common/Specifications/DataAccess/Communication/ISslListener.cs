using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;

namespace ExternalServer.Common.Specifications.DataAccess.Communication {

    public delegate void SslStreamOpenCallback(SslStream stream, TcpClient tcpClient);

    public interface ISslListener {

        /// <summary>
        /// Starts listening on <paramref name="endPoint"/>.
        /// Calls <paramref name="sslStreamOpenCallback"/> when a client connected.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="endPoint"></param>
        /// <param name="sslStreamOpenCallback">Must close the sslStream at the end.</param>
        /// <param name="keepAliveInterval">0 or less, to deactivate keep alive. Value in ms.</param>
        void Start(CancellationToken token, IPEndPoint endPoint, SslStreamOpenCallback sslStreamOpenCallback, int keepAliveInterval);
    }
}
