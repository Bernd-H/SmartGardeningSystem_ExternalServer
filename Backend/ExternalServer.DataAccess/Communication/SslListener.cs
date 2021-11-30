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
using NLog;

namespace ExternalServer.DataAccess.Communication {
    public class SslListener : ISslListener {

        private static ManualResetEvent tcpClientConnected = new ManualResetEvent(false);

        private TcpListener _tcpListener;

        private Thread _workingThread;

        private SslStreamOpenCallback _sslStreamOpenCallback;

        private int _keepAliveInterval;


        private ICertificateManager CertificateManager;

        private ILogger Logger;

        public SslListener(ILoggerService loggerService, ICertificateManager certificateManager) {
            Logger = loggerService.GetLogger<SslListener>();
            CertificateManager = certificateManager;
        }

        public void Start(CancellationToken token, IPEndPoint endPoint, SslStreamOpenCallback sslStreamOpenCallback, int keepAliveInterval) {
            _sslStreamOpenCallback = sslStreamOpenCallback;
            _keepAliveInterval = keepAliveInterval;

            _workingThread = new Thread(() => {
                _tcpListener = new TcpListener(endPoint);
                _tcpListener.Server.ReceiveTimeout = 1000; // 1s
                _tcpListener.Server.SendTimeout = 1000;

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
                sslStream.ReadTimeout = 1000;
                sslStream.WriteTimeout = 1000;

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

        public static void SendMessage(SslStream sslStream, byte[] msg) {
            List<byte> packet = new List<byte>();

            // add length of packet - 4B
            packet.AddRange(BitConverter.GetBytes(msg.Length + 4));

            // add content
            packet.AddRange(msg);

            sslStream.Write(packet.ToArray());
            sslStream.Flush();
        }

        public static byte[] ReadMessage(SslStream sslStream) {
            int bytes = -1;
            int packetLength = -1;
            int readBytes = 0;
            List<byte> packet = new List<byte>();

            do {
                byte[] buffer = new byte[2048];
                bytes = sslStream.Read(buffer, 0, buffer.Length);

                // get length
                if (packetLength == -1) {
                    byte[] length = new byte[4];
                    Array.Copy(buffer, 0, length, 0, 4);
                    packetLength = BitConverter.ToInt32(length, 0);
                }

                readBytes += bytes;
                packet.AddRange(buffer);

            } while (bytes != 0 && packetLength - readBytes > 0);

            // remove length information and attached bytes
            packet.RemoveRange(packetLength, packet.Count - packetLength);
            packet.RemoveRange(0, 4);

            return packet.ToArray();
        }
    }
}
