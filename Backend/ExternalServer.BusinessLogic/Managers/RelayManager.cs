using System;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExternalServer.Common.Configuration;
using ExternalServer.Common.Models;
using ExternalServer.Common.Specifications;
using ExternalServer.Common.Specifications.DataAccess.Communication;
using ExternalServer.Common.Specifications.Managers;
using ExternalServer.Common.Utilities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NLog;

namespace ExternalServer.BusinessLogic.Managers {
    public class RelayManager : IRelayManager {

        private CancellationToken _cancellationToken;


        private IConnectionsManager ConnectionsManager;

        private ISslListener SslListener;

        private IConfiguration Configuration;

        private ILogger Logger;

        public RelayManager(ILoggerService loggerService, ISslListener sslListener, IConnectionsManager connectionsManager, IConfiguration configuration) {
            Logger = loggerService.GetLogger<RelayManager>();
            SslListener = sslListener;
            ConnectionsManager = connectionsManager;
            Configuration = configuration;
        }

        public void Start(CancellationToken cancellationToken) {
            _cancellationToken = cancellationToken;
            int port = Convert.ToInt32(Configuration[ConfigurationVars.MOBILEAPPRELAYSERVICE_PORT]);
            Logger.Info($"[Start]Starting RelayManager.");
            SslListener.Start(cancellationToken, new IPEndPoint(IPAddress.Any, port), MoblieAppConnected, keepAliveInterval: 0, receiveTimeout: -1);
        }

        private void MoblieAppConnected(SslStream stream, TcpClient tcpClient) {
            Task.Run(() => {
                Logger.Info($"[MoblieAppConnected]Client with IP={tcpClient.Client.RemoteEndPoint} connected.");

                try {
                    // get the Basestation Id the client want's to connect to
                    var basestationIdBytes = DataAccess.Communication.SslListener.ReadMessage(stream);
                    var basestationId = new Guid(basestationIdBytes);

                    Logger.Info($"[MoblieAppConnected]Forwarding connect request to basestation with id={basestationId}.");

                    // send conenction request to basestation
                    var relayRequestResult = ConnectionsManager.SendUserConnectRequest(basestationId);

                    // send client back IRelayRequestResult
                    byte[] answer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(relayRequestResult.ToDto()));
                    DataAccess.Communication.SslListener.SendMessage(stream, answer);

                    var ack = DataAccess.Communication.SslListener.ReadMessage(stream);
                    if (!ack.SequenceEqual(CommunicationCodes.ACK)) {
                        return;
                    }

                    if (relayRequestResult.basestationStream != null && relayRequestResult.PeerToPeerEndPoint == null) {
                        // peer to peer connection is not possible due to the basestations NAT
                        Logger.Info($"[MoblieAppConnected]Relaying packages from {tcpClient.Client.RemoteEndPoint} to {basestationId}.");

                        while (true) {
                            // tunnel messages
                            var bytes_fromClient = DataAccess.Communication.SslListener.ReadMessage(stream);
                            Logger.Info($"[MoblieAppConnected]Tunneling {bytes_fromClient.Length} bytes to basestation.");
                            DataAccess.Communication.SslListener.SendMessage(relayRequestResult.basestationStream, bytes_fromClient);
                            var bytes_fromBasestation = DataAccess.Communication.SslListener.ReadMessage(relayRequestResult.basestationStream);
                            Logger.Info($"[MobileAppConnected]Tunneling {bytes_fromBasestation.Length} bytes to client.");
                            DataAccess.Communication.SslListener.SendMessage(stream, bytes_fromBasestation);
                        }
                    }
                    else if (relayRequestResult.basestationStream != null && relayRequestResult.PeerToPeerEndPoint != null) {
                        Logger.Info($"[MoblieAppConnected]Sent {tcpClient.Client.RemoteEndPoint} a peer to peer endpoint from {basestationId}.");
                    }
                    else {
                        // basestation is not connected to the server
                        // connection will get closed at the end
                    }
                }
                catch (Exception ex) {
                    Logger.Error(ex, $"[MobileAppConnected]An error occured.");
                }
            }, _cancellationToken).ContinueWith(task => {
                // closes also the underlying stream
                stream?.Close();
            }).Wait();
        }
    }
}
