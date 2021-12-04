using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ExternalServer.Common.Configuration;
using ExternalServer.Common.Models.Entities;
using ExternalServer.Common.Specifications;
using ExternalServer.Common.Specifications.DataAccess.Communication;
using ExternalServer.Common.Specifications.DataObjects;
using ExternalServer.Common.Specifications.Managers;
using ExternalServer.Common.Utilities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NLog;

namespace ExternalServer.BusinessLogic.Managers {

    public class ConnectionsManager : IConnectionsManager {

        private class Connection {
            public SslStream stream { get; set; }

            public TcpClient client { get; set; }
        }

        private Dictionary<Guid, Connection> _connections;

        private IConfiguration Configuration;

        private ISslListener SslListener;

        private ILogger Logger;

        public ConnectionsManager(ILoggerService loggerService, ISslListener sslListener, IConfiguration configuration) {
            Logger = loggerService.GetLogger<ConnectionsManager>();
            SslListener = sslListener;
            Configuration = configuration;

            _connections = new Dictionary<Guid, Connection>();
        }

        public void CleanupGhostConenctions() {
            Logger.Info($"[CleanupGhostConenctions]Checking connections.");
            lock (_connections) {
                List<Guid> connectionsToRemove = new List<Guid>();

                foreach (var c in _connections) {
                    if (!c.Value.client.Client.IsConnected()) {
                        connectionsToRemove.Add(c.Key);
                    }
                }

                foreach (var idToRemove in connectionsToRemove) {
                    _connections.Remove(idToRemove);
                }
            }
        }

        public IRelayRequestResult SendUserConnectRequest(Guid basestationId) {
            try {
                if (_connections.ContainsKey(basestationId)) {
                    var connection = _connections[basestationId];
                    if (connection.client.Client.IsConnected_UsingPoll()) {

                        lock (connection) { // only one client can establish a connection with the basestation at the same time
                            // notify basesation and ask if peer to peer is possible
                            var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new WanPackage {
                                Package = CommunicationCodes.SendPeerToPeerEndPoint,
                                PackageType = PackageType.Init
                            }));
                            DataAccess.Communication.SslListener.SendMessage(connection.stream, buffer);

                            var answer = DataAccess.Communication.SslListener.ReadMessage(connection.stream);
                            var answerO = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(answer)) as WanPackage;

                            if (answerO.Package.Length != 0) { // no peer to peer possible
                                // relay mobile app traffic over this server
                                return new RelayRequestResult() {
                                    basestationStream = connection.stream
                                };
                            }
                            else {
                                // basestation managed to open a publicly accessable port
                                return new RelayRequestResult() {
                                    PeerToPeerEndPoint = IPEndPoint.Parse(Encoding.UTF8.GetString(answerO.Package)),
                                    basestationStream = connection.stream
                                };
                            }
                        }
                    }
                    else {
                        // remove client form list
                        RemoveConnection(basestationId);
                    }
                }
            }
            catch (SocketException) { }
            catch (ObjectDisposedException) { }
            catch (Exception ex) {
                Logger.Error(ex, $"[SendUserConnectRequest]Failed. Removing connection from list.");
                RemoveConnection(basestationId);
            }

            return new RelayRequestResult();
        }

        public void Start(CancellationToken token) {
            int port = Convert.ToInt32(Configuration[ConfigurationVars.BASESTATIONCONNECTIONSERVICE_PORT]);
            SslListener.Start(token, new IPEndPoint(IPAddress.Any, port), ClientConnected, 0);
        }

        private void ClientConnected(SslStream stream, TcpClient client) {
            try {
                // receive id
                var basestationId = DataAccess.Communication.SslListener.ReadMessage(stream);

                // send ack
                DataAccess.Communication.SslListener.SendMessage(stream, CommunicationCodes.ACK);

                Logger.Info($"[ClientConnected]Accommodate bastation with id={basestationId} to the list.");

                lock (_connections) {
                    _connections.Add(new Guid(basestationId), new Connection {
                        client = client,
                        stream = stream
                    });
                }

                // connection will stay open
            }
            catch (Exception ex) {
                Logger.Error(ex, "[ClientConnected]Failed to receive a valid id from a basestation.");
                // this stream will also close the underlying client
                stream?.Close();
            }
        }

        private void RemoveConnection(Guid basestationId) {
            lock (_connections) {
                _connections.Remove(basestationId);
            }
        }
    }
}
