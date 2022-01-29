using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using ExternalServer.Common.Events.Communication;
using ExternalServer.Common.Exceptions;

namespace ExternalServer.Common.Specifications.DataAccess.Communication {

    /// <summary>
    /// Callback delegate for an open ssl stream.
    /// </summary>
    /// <param name="stream">SslStream instance.</param>
    /// <param name="tcpClient">Underlying tcp client.</param>
    public delegate void SslStreamOpenCallback(SslStream stream, TcpClient tcpClient);

    /// <summary>
    /// A Tcp listener that sends and receives all packages over a ssl stream.
    /// </summary>
    public interface ISslListener {

        /// <summary>
        /// Event that gets raised when a client connected successfully.
        /// </summary>
        event EventHandler<ClientConnectedEventArgs> ClientConnectedEventHandler;

        /// <summary>
        /// Starts listening on <paramref name="endPoint"/>.
        /// Raises the ClientConnectedEventHandler event when a client connected.
        /// </summary>
        /// <param name="token">Cancellation token to stop listening and to close open connections.</param>
        /// <param name="endPoint">Local endpoint to listen on.</param>
        /// <param name="keepAliveInterval">0 or less, to deactivate keep alive. Value in s.</param>
        /// <param name="receiveTimeout">Receive-Timeout in milliseconds.</param>
        void Start(CancellationToken token, IPEndPoint endPoint, int keepAliveInterval, int receiveTimeout);

        /// <summary>
        /// Sends data over the ssl stream to the client.
        /// </summary>
        /// <param name="sslStream">Open ssl stream to the client.</param>
        /// <param name="msg">Data to send.</param>
        /// <exception cref="ConnectionClosedException">Gets thrown when the cancellation got requested through the CancellationToken passed in Start().</exception>
        void SendMessage(SslStream sslStream, byte[] msg);

        /// <summary>
        /// Receives data from the <paramref name="sslStream"/>.
        /// Aborts the operation when the cancellation got requested through the CancellationToken passed in Start().
        /// </summary>
        /// <param name="sslStream">Open ssl stream.</param>
        /// <returns>Received byte array.</returns>
        byte[] ReadMessage(SslStream sslStream);
    }
}
