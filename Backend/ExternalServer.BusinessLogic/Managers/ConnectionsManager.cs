using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ExternalServer.Common.Specifications;
using ExternalServer.Common.Specifications.DataAccess.Communication;
using ExternalServer.Common.Specifications.Managers;
using ExternalServer.Common.Utilities;
using NLog;

namespace ExternalServer.BusinessLogic.Managers {

    public class ConnectionsManager : IConnectionsManager {

        private class Connection {
            public SslStream stream { get; set; }

            public TcpClient client { get; set; }
        }

        private Dictionary<Guid, Connection> _connections;


        private ISslListener SslListener;

        private ILogger Logger;

        public ConnectionsManager(ILoggerService loggerService, ISslListener sslListener) {
            Logger = loggerService.GetLogger<ConnectionsManager>();
            SslListener = sslListener;

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

        public IPEndPoint SendUserConnectRequest(Guid basestationId) {
            try {
                if (_connections.ContainsKey(basestationId)) {
                    var connection = _connections[basestationId];
                    if (connection.client.Client.IsConnected()) {
                        // notify client and ask if peer to peer is possible
                        DataAccess.Communication.SslListener.SendMessage(connection.stream, CommunicationCodes.SendPeerToPeerEndPoint);

                        var endPoint = DataAccess.Communication.SslListener.ReadMessage(connection.stream);

                        if (endPoint.SequenceEqual(CommunicationCodes.NoPeerToPeerPossible)) {
                            // relay mobile app traffic over this server
                            return InitiateRelay(connection);
                        }
                        else {
                            // basestation managed to open a publicly accessable port
                            return IPEndPoint.Parse(Encoding.UTF8.GetString(endPoint));
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

            return null;
        }

        public void Start(CancellationToken token) {
            SslListener.Start(token, new IPEndPoint(IPAddress.Any, 5039), ClientConnected, 60000);
        }

        private void ClientConnected(SslStream stream, TcpClient client) {
            try {
                // receive key
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

        private IPEndPoint InitiateRelay(Connection connection) {
            throw new NotImplementedException();
        }

        private void RemoveConnection(Guid basestationId) {
            lock (_connections) {
                _connections.Remove(basestationId);
            }
        }
    }
}
