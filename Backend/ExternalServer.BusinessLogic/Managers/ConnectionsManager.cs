using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ExternalServer.Common.Configuration;
using ExternalServer.Common.Events.Communication;
using ExternalServer.Common.Models;
using ExternalServer.Common.Models.DTOs;
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

    /// <inheritdoc/>
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

        /// <inheritdoc/>
        public void CleanupGhostConenctions() {
            Logger.Info($"[CleanupGhostConenctions]Checking connections.");
            lock (_connections) {
                List<Guid> connectionsToRemove = new List<Guid>();

                foreach (var c in _connections) {
                    //if (!c.Value.client.Client.IsConnected()) {
                    if (!c.Value.client.Client.IsConnected_UsingPoll()) {
                        connectionsToRemove.Add(c.Key);
                    }
                }

                Logger.Info($"[CleanupGhostConenctions]Removing {connectionsToRemove.Count} connections.");
                foreach (var idToRemove in connectionsToRemove) {
                    _connections.Remove(idToRemove);
                }
            }
        }

        /// <inheritdoc/>
        public IConnectRequestResult SendUserConnectRequest(ConnectRequest connectRequest) {
            var basestationId = connectRequest.BasestationId;

            try {
                if (_connections.ContainsKey(basestationId)) {
                    var connection = _connections[basestationId];
                    if (connection.client.Client.IsConnected_UsingPoll()) {

                        lock (connection) { // only one client can establish a connection with the basestation at the same time
                            // notify basesation and ask if peer to peer is possible
                            var buffer = CommunicationUtils.SerializeObject(new WanPackage {
                                Package = CommunicationUtils.SerializeObject<ConnectRequestDto>(connectRequest.ToDto()),
                                PackageType = PackageType.Init
                            });
                            SslListener.SendMessage(connection.stream, buffer);

                            var answer = SslListener.ReadMessage(connection.stream);
                            var answerO = CommunicationUtils.DeserializeObject<WanPackage>(answer);

                            if (answerO.PackageType == PackageType.PeerToPeerInit) {
                                // basestation managed to open a publicly accessable port
                                return new ConnectRequestResult() {
                                    PeerToPeerEndPoint = IPEndPoint.Parse(Encoding.UTF8.GetString(answerO.Package)),
                                    basestationStream = connection.stream
                                };
                            }
                            else if (answerO.PackageType == PackageType.ExternalServerRelayInit) {
                                // no peer to peer possible
                                // relay mobile app traffic over this server
                                return new ConnectRequestResult() {
                                    TunnelId = new Guid(answerO.Package),
                                    basestationStream = connection.stream
                                };
                            }
                        }
                    }
                    else {
                        // remove client form list
                        Logger.Info($"[SendUserConnectRequest]Removing basestation because it didn't respond to poll.");
                        RemoveConnection(basestationId); // should i really do that?
                    }
                }
            }
            catch (SocketException sex) {
                Logger.Error(sex, $"[SendUserConnectRequest]A socket exception occured.");
            }
            catch (ObjectDisposedException odex) {
                Logger.Error(odex, $"[SendUserConnectRequest]{odex.ObjectName} disposed.");
            }
            catch (Exception ex) {
                Logger.Error(ex, $"[SendUserConnectRequest]Failed. Removing connection from list.");
                //RemoveConnection(basestationId); // should i really do that?
            }

            return new ConnectRequestResult();
        }

        /// <inheritdoc/>
        public void Start(CancellationToken token) {
            int port = Convert.ToInt32(Configuration[ConfigurationVars.BASESTATIONCONNECTIONSERVICE_PORT]);
            SslListener.ClientConnectedEventHandler += ClientConnected;
            SslListener.Start(token, new IPEndPoint(IPAddress.Any, port), keepAliveInterval: 60, receiveTimeout: 20000);
            //SslListener.Start(token, new IPEndPoint(IPAddress.Any, port), ClientConnected, keepAliveInterval: 60, receiveTimeout: 20000);
        }

        private void ClientConnected(object o, ClientConnectedEventArgs args) {
            SslStream stream = (SslStream)args.Stream;
            TcpClient client = args.TcpClient;

            try {
                // receive id
                var basestationIdBytes = SslListener.ReadMessage(stream);
                var basestationId = new Guid(basestationIdBytes);

                // send ack
                SslListener.SendMessage(stream, CommunicationCodes.ACK);

                Logger.Info($"[ClientConnected]Accommodating bastation with id={basestationId.ToString()} to the list.");

                lock (_connections) {
                    if (_connections.ContainsKey(basestationId)) {
                        _connections[basestationId].stream?.Close();
                        _connections.Remove(basestationId);
                    }

                    _connections.Add(basestationId, new Connection {
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
