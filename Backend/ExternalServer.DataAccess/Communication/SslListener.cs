using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Threading;
using ExternalServer.Common.Events.Communication;
using ExternalServer.Common.Specifications;
using ExternalServer.Common.Specifications.DataAccess.Communication;
using ExternalServer.Common.Specifications.Managers;
using ExternalServer.Common.Utilities;
using NLog;

namespace ExternalServer.DataAccess.Communication {

    /// <inheritdoc/>
    public class SslListener : ISslListener {

        /// <inheritdoc/>

        public event EventHandler<ClientConnectedEventArgs> ClientConnectedEventHandler;


        private TcpListener _tcpListener;

        private Thread _workingThread;

        private int _keepAliveInterval;

        private int _receiveTimeout;

        private CancellationToken _cancellationToken;


        private ICertificateManager CertificateManager;

        private ILogger Logger;

        public SslListener(ILoggerService loggerService, ICertificateManager certificateManager) {
            Logger = loggerService.GetLogger<SslListener>();
            CertificateManager = certificateManager;
        }

        /// <inheritdoc/>
        public void Start(CancellationToken token, IPEndPoint endPoint, int keepAliveInterval, int receiveTimeout) {
            _keepAliveInterval = keepAliveInterval;
            _receiveTimeout = receiveTimeout;
            _cancellationToken = token;

            _workingThread = new Thread(() => {
                _tcpListener = new TcpListener(endPoint);
                _tcpListener.Server.ReceiveTimeout = receiveTimeout;
                _tcpListener.Server.SendTimeout = 5000;

                _tcpListener.Start();
                token.Register(() => _tcpListener.Stop());

                Logger.Info($"[Start]Listening on {_tcpListener.LocalEndpoint}...");

                try {
                    while (!token.IsCancellationRequested) {
                        //Logger.Info($"[Start]Waiting to accept a tcp client on thread: {System.Threading.Thread.CurrentThread.ManagedThreadId}.");
                        var client = _tcpListener.AcceptTcpClient();

                        HandleNewClient(client);
                    }
                }catch (SocketException) {
                    // _tcpListener gets stopped
                    Logger.Info($"[Start]Stopped accepting new tcp clients.");
                    return;
                }
            });

            _workingThread.Start();
        }

        private void HandleNewClient(TcpClient client) {
            // handle this client with a thread from the thread pool
            ThreadPool.QueueUserWorkItem(state => {
                SslStream sslStream = null;

                try {
                    //var remotePort = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
                    //Logger.Info($"[Start]Thread Id while establishing the ssl stream for client with remote port {remotePort}" +
                    //    $" = {System.Threading.Thread.CurrentThread.ManagedThreadId}");

                    client.Client.Blocking = true;
                    ConfigureKeepAlive(client);

                    // open ssl stream
                    sslStream = new SslStream(client.GetStream(), false);

                    // Set timeouts for the read and write to 1 second.
                    sslStream.ReadTimeout = _receiveTimeout;
                    sslStream.WriteTimeout = 5000;

                    sslStream.AuthenticateAsServer(CertificateManager.GetCertificate(), clientCertificateRequired: false, checkCertificateRevocation: true);

                    // communicate
                    ClientConnectedEventHandler?.Invoke(null, new ClientConnectedEventArgs(client, sslStream));
                }
                catch (AuthenticationException e) {
                    Logger.Error(e, "[AcceptTcpClientCallback]Authentication failed - closing the connection.");
                    sslStream?.Close();
                }
                catch (ObjectDisposedException odex) {
                    Logger.Trace($"[AcceptTcpClientCallback]Connection from {client?.Client.RemoteEndPoint} got unexpectedly closed.");
                    sslStream?.Close();
                }
                catch (Exception ex) {
                    Logger.Trace(ex, "[AcceptTcpClientCallback]An excpetion occured.");
                    sslStream?.Close();
                }
                finally {
                    // ssl Stream will get close in the callback
                }
            });
        }

        private void ConfigureKeepAlive(TcpClient client) {
            if (_keepAliveInterval > 0) {
                Logger.Info($"[ConfigureKeepAlive]Settings keep alive interval to {_keepAliveInterval}s for connection with endpoint {client.Client.RemoteEndPoint}.");

                client.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, _keepAliveInterval);
                client.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 5);
                client.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 2);
                //client.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.KeepAlive, true);
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            }
        }

        /// <inheritdoc/>
        public void SendMessage(SslStream sslStream, byte[] msg) {
            // use the async method to be able to cancel the send process
            CommunicationUtils.SendAsync(Logger, msg, sslStream, _cancellationToken).Wait();
        }

        /// <inheritdoc/>
        public byte[] ReadMessage(SslStream sslStream) {
            // use the async method to be able to cancellate the receive process
            return CommunicationUtils.ReceiveAsync(Logger, sslStream, _cancellationToken).Result;
        }

        #region old BeginAccept methods and ConfigureKeepAlive

        private ManualResetEvent tcpClientConnected = new ManualResetEvent(false);

        public void Start_old(CancellationToken token, IPEndPoint endPoint, int keepAliveInterval, int receiveTimeout) {
            //_sslStreamOpenCallback = sslStreamOpenCallback;
            _keepAliveInterval = keepAliveInterval;
            _receiveTimeout = receiveTimeout;

            _workingThread = new Thread(() => {
                _tcpListener = new TcpListener(endPoint);
                _tcpListener.Server.ReceiveTimeout = receiveTimeout;
                _tcpListener.Server.SendTimeout = 5000;

                _tcpListener.Start();
                token.Register(() => _tcpListener.Stop());

                Logger.Info($"[Start]Listening on {_tcpListener.LocalEndpoint}...");

                while (!token.IsCancellationRequested) {
                    tcpClientConnected.Reset();

                    _tcpListener.BeginAcceptTcpClient(BeginAcceptClient, token);

                    tcpClientConnected.WaitOne();
                }
            });

            _workingThread.Start();
        }

        private void BeginAcceptClient(IAsyncResult ar) {
            SslStream sslStream = null;
            TcpClient client = null;
            CancellationToken token = (CancellationToken)ar.AsyncState;
            bool mainThreadBlockReleased = false;

            try {
                if (token.IsCancellationRequested)
                    return;

                // Get and configure client
                client = _tcpListener.EndAcceptTcpClient(ar);
                client.Client.Blocking = true;
                ConfigureKeepAlive(client);

                Logger.Info($"[BeginAcceptClient]ThreadId: {System.Threading.Thread.CurrentThread.ManagedThreadId}.");

                tcpClientConnected.Set();
                mainThreadBlockReleased = true;

                // open ssl stream
                sslStream = new SslStream(client.GetStream(), false);

                // Set timeouts for the read and write to 1 second.
                sslStream.ReadTimeout = _receiveTimeout;
                sslStream.WriteTimeout = 5000;

                sslStream.AuthenticateAsServer(CertificateManager.GetCertificate(), clientCertificateRequired: false, checkCertificateRevocation: true);

                // communicate
                //_sslStreamOpenCallback.Invoke(sslStream, client);
                ClientConnectedEventHandler?.Invoke(null, new ClientConnectedEventArgs(client, sslStream));
            }
            catch (AuthenticationException e) {
                Logger.Error(e, "[AcceptTcpClientCallback]Authentication failed - closing the connection.");
                sslStream?.Close();
            }
            catch (ObjectDisposedException odex) {
                Logger.Trace($"[AcceptTcpClientCallback]Connection from {client?.Client.RemoteEndPoint} got unexpectedly closed.");
                sslStream?.Close();
            }
            catch (Exception ex) {
                Logger.Trace(ex, "[AcceptTcpClientCallback]An excpetion occured.");
                sslStream?.Close();
            }
            finally {
                // ssl Stream will get close in the callback
                // sslStream?.Close();

                if (!mainThreadBlockReleased) {
                    tcpClientConnected.Set();
                }
            }
        }

        /// <summary>
        /// Does not work on linux!
        /// </summary>
        /// <param name="client"></param>
        [Obsolete]
        private void ConfigureKeepAlive_old(TcpClient client) {
            if (_keepAliveInterval > 0) {
                Logger.Info($"[ConfigureKeepAlive]Settings keep alive interval to {_keepAliveInterval}ms for connection with endpoint {client.Client.RemoteEndPoint}.");

                // Get the size of the uint to use to back the byte array
                int size = Marshal.SizeOf((uint)0);

                // Create the byte array
                byte[] keepAlive = new byte[size * 3];

                // Pack the byte array:
                // Turn keepalive on
                Buffer.BlockCopy(BitConverter.GetBytes((uint)1), 0, keepAlive, 0, size);
                // Set amount of time without activity before sending a keepalive to 5 seconds
                Buffer.BlockCopy(BitConverter.GetBytes((uint)5000), 0, keepAlive, size, size);
                // Set keepalive interval to 5 seconds
                Buffer.BlockCopy(BitConverter.GetBytes((uint)5000), 0, keepAlive, size * 2, size);

                // Set the keep-alive settings on the underlying Socket
                client.Client.IOControl(IOControlCode.KeepAliveValues, keepAlive, null);
            }
        }

        #endregion
    }
}
