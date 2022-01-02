using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Threading;
using ExternalServer.Common.Specifications;
using ExternalServer.Common.Specifications.DataAccess.Communication;
using ExternalServer.Common.Specifications.Managers;
using ExternalServer.Common.Utilities;
using NLog;

namespace ExternalServer.DataAccess.Communication {
    public class SslListener : ISslListener {

        private ManualResetEvent tcpClientConnected = new ManualResetEvent(false);

        private TcpListener _tcpListener;

        private Thread _workingThread;

        private SslStreamOpenCallback _sslStreamOpenCallback;

        private int _keepAliveInterval;

        private int _receiveTimeout;


        private ICertificateManager CertificateManager;

        private ILogger Logger;

        public SslListener(ILoggerService loggerService, ICertificateManager certificateManager) {
            Logger = loggerService.GetLogger<SslListener>();
            CertificateManager = certificateManager;
        }

        public void Start(CancellationToken token, IPEndPoint endPoint, SslStreamOpenCallback sslStreamOpenCallback, int keepAliveInterval, int receiveTimeout) {
            _sslStreamOpenCallback = sslStreamOpenCallback;
            _keepAliveInterval = keepAliveInterval;
            _receiveTimeout = receiveTimeout;

            _workingThread = new Thread(() => {
                _tcpListener = new TcpListener(endPoint);
                _tcpListener.Server.ReceiveTimeout = receiveTimeout;
                _tcpListener.Server.SendTimeout = 5000;

                _tcpListener.Start();
                token.Register(() => _tcpListener.Stop());

                Logger.Info($"[Start]Listening on {_tcpListener.LocalEndpoint}...");

                while (true) {
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

            try {
                if (token.IsCancellationRequested)
                    return;

                // Get and configure client
                client = _tcpListener.EndAcceptTcpClient(ar);
                client.Client.Blocking = true;
                ConfigureKeepAlive(client);

                tcpClientConnected.Set();

                // open ssl stream
                sslStream = new SslStream(client.GetStream(), false);

                // Set timeouts for the read and write to 1 second.
                sslStream.ReadTimeout = _receiveTimeout;
                sslStream.WriteTimeout = 5000;

                sslStream.AuthenticateAsServer(CertificateManager.GetCertificate(), clientCertificateRequired: false, checkCertificateRevocation: true);

                // communicate
                _sslStreamOpenCallback.Invoke(sslStream, client);
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
            }
        }

        private void ConfigureKeepAlive(TcpClient client) {
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

        public void SendMessage(SslStream sslStream, byte[] msg) {
            CommunicationUtils.Send(Logger, msg, sslStream);
        }

        public byte[] ReadMessage(SslStream sslStream) {
            return CommunicationUtils.Receive(Logger, sslStream);
        }
    }
}
