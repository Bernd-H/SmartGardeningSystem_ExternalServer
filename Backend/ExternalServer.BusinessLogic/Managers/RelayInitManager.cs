using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExternalServer.Common.Configuration;
using ExternalServer.Common.Events.Communication;
using ExternalServer.Common.Exceptions;
using ExternalServer.Common.Models;
using ExternalServer.Common.Models.DTOs;
using ExternalServer.Common.Specifications;
using ExternalServer.Common.Specifications.DataAccess.Communication;
using ExternalServer.Common.Specifications.Managers;
using ExternalServer.Common.Utilities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NLog;

namespace ExternalServer.BusinessLogic.Managers {

    /// <inheritdoc/>
    public class RelayInitManager : IRelayInitManager {

        private CancellationToken _cancellationToken;


        private IConnectionsManager ConnectionsManager;

        private ISslListener SslListener;

        private IRelayManager RelayManager;

        private IConfiguration Configuration;

        private ILogger Logger;

        public RelayInitManager(ILoggerService loggerService, ISslListener sslListener, IConnectionsManager connectionsManager, IConfiguration configuration,
            IRelayManager relayManager) {
            Logger = loggerService.GetLogger<RelayInitManager>();
            SslListener = sslListener;
            ConnectionsManager = connectionsManager;
            Configuration = configuration;
            RelayManager = relayManager;
        }

        /// <inheritdoc/>
        public void Start(CancellationToken cancellationToken) {
            _cancellationToken = cancellationToken;
            int port = Convert.ToInt32(Configuration[ConfigurationVars.MOBILEAPPRELAYSERVICE_PORT]);
            Logger.Info($"[Start]Starting RelayInitializationManager.");
            SslListener.ClientConnectedEventHandler += OnMoblieAppConnected;
            SslListener.Start(cancellationToken, new IPEndPoint(IPAddress.Any, port), keepAliveInterval: 0, receiveTimeout: System.Threading.Timeout.Infinite);
        }

        private void OnMoblieAppConnected(object o, ClientConnectedEventArgs clientConnectedEventArgs) {
            //var remotePort = ((IPEndPoint)clientConnectedEventArgs.TcpClient.Client.RemoteEndPoint).Port;
            //Logger.Info($"[OnMoblieAppConnected]Id of the thread in the event for client with remote port {remotePort} = {System.Threading.Thread.CurrentThread.ManagedThreadId}.");

            SslStream stream = (SslStream)clientConnectedEventArgs.Stream;
            TcpClient tcpClient = clientConnectedEventArgs.TcpClient;

            var remoteEndPoint = tcpClient.Client.RemoteEndPoint;
            SslStream tunnelStream = null;
            Guid basestationId = Guid.Empty;

            Logger.Info($"[OnMoblieAppConnected]Client with IP={remoteEndPoint} connected.");

            try {
                // get the Basestation Id the client want's to connect to
                var receivedBytes = SslListener.ReadMessage(stream);
                var connectRequest = CommunicationUtils.DeserializeObject<ConnectRequestDto>(receivedBytes).FromDto();
                basestationId = connectRequest.BasestationId;

                Logger.Info($"[OnMoblieAppConnected]Forwarding connect request to basestation with id={basestationId}.");

                // send conenction request to basestation
                var relayRequestResult = ConnectionsManager.SendUserConnectRequest(connectRequest);

                // send client back IRelayRequestResult
                byte[] answer = CommunicationUtils.SerializeObject(relayRequestResult.ToDto());
                SslListener.SendMessage(stream, answer);

                var ack = SslListener.ReadMessage(stream);
                if (!ack.SequenceEqual(CommunicationCodes.ACK)) {
                    return;
                }

                if (relayRequestResult.basestationStream != null && relayRequestResult.TunnelId != Guid.Empty) {
                    // peer to peer connection is not possible due to the basestations NAT
                    // get open tunnel connection to the basestation
                    var basestationStream = RelayManager.GetTunnelToBasestation(relayRequestResult.TunnelId);
                    if (basestationStream != null) {
                        tunnelStream = basestationStream;
                        Logger.Info($"[OnMoblieAppConnected]Relaying packages from {tcpClient.Client.RemoteEndPoint} to {basestationId}" +
                            $" via tunnel with id {relayRequestResult.TunnelId}.");

                        while (!_cancellationToken.IsCancellationRequested) {
                            // tunnel messages
                            var bytes_fromClient = SslListener.ReadMessage(stream);

                            #region For debugging the SendLargePacketError20220130
                            //if (!File.Exists("ExternalServer_receivedMsg.bin")) {
                            //    // store the first received package for test reasons...
                            //    Logger.Info($"[OnMoblieAppConnected]Storing the received package in ExternalServer_receivedMsg.bin");
                            //    File.WriteAllBytes("ExternalServer_receivedMsg.bin", bytes_fromClient);
                            //}
                            #endregion

                            Logger.Info($"[OnMoblieAppConnected]Tunneling {bytes_fromClient.Length} bytes to basestation.");
                            SslListener.SendMessage(basestationStream, bytes_fromClient);
                            var bytes_fromBasestation = SslListener.ReadMessage(basestationStream);
                            Logger.Info($"[OnMoblieAppConnected]Tunneling {bytes_fromBasestation.Length} bytes to client.");
                            SslListener.SendMessage(stream, bytes_fromBasestation);
                        }
                    }
                    else {
                        Logger.Error($"[OnMoblieAppConnected]Stream for tunnelId={relayRequestResult.TunnelId} was null.");
                    }
                }
                else if (relayRequestResult.basestationStream != null && relayRequestResult.PeerToPeerEndPoint != null) {
                    Logger.Info($"[OnMoblieAppConnected]Sent to {tcpClient.Client.RemoteEndPoint} a peer to peer endpoint from {basestationId}.");
                }
                else {
                    // basestation is not connected to the server
                    // connection will get closed at the end
                }
            }
            catch (ConnectionClosedException) {
                Logger.Info($"[OnMoblieAppConnected]Mobile app (ep={remoteEndPoint}) or basestation (id={basestationId}) closed the connection.");
            }
            catch (Exception ex) {
                Logger.Trace(ex, $"[OnMoblieAppConnected]An error occured (ep={remoteEndPoint}).");
            }
            finally {
                // close tunnel connection to the basestation
                tunnelStream?.Close();
            }

            // closes also the underlying stream
            stream?.Close();
        }
    }
}
