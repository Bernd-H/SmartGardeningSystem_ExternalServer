using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using ExternalServer.Common.Configuration;
using ExternalServer.Common.Specifications;
using ExternalServer.Common.Specifications.DataAccess.Communication;
using ExternalServer.Common.Specifications.Managers;
using ExternalServer.Common.Utilities;
using Microsoft.Extensions.Configuration;
using NLog;

namespace ExternalServer.BusinessLogic.Managers {
    public class RelayManager : IRelayManager {

        private Dictionary<Guid, SslStream> openTunnels = new Dictionary<Guid, SslStream>();


        private ISslListener SslListener;

        private IConfiguration Configuration;

        private ILogger Logger;

        public RelayManager(ILoggerService loggerService, IConfiguration configuration, ISslListener sslListener) {
            Logger = loggerService.GetLogger<RelayManager>();
            Configuration = configuration;
            SslListener = sslListener;
        }

        public void Start(CancellationToken cancellationToken) {
            cancellationToken.Register(() => Stop());
            int port = Convert.ToInt32(Configuration[ConfigurationVars.BASESTATIONRELAYTUNNEL_PORT]);
            Logger.Info($"[Start]Starting RelayManager.");
            SslListener.Start(cancellationToken, new IPEndPoint(IPAddress.Any, port), BasestationConnected, keepAliveInterval: 0, receiveTimeout: System.Threading.Timeout.Infinite);
        }

        public SslStream GetTunnelToBasestation(Guid tunnelId) {
            if (openTunnels.ContainsKey(tunnelId)) {
                return openTunnels[tunnelId];
            }

            return null;
        }

        private void BasestationConnected(SslStream stream, TcpClient tcpClient) {
            var remoteEndPoint = tcpClient.Client.RemoteEndPoint;
            Logger.Info($"[BasestationConnected]Basestation with IP={remoteEndPoint} connected.");

            try {
                // get the Basestation Id the client want's to connect to
                var receivedBytes = SslListener.ReadMessage(stream);
                var tunnelId = new Guid(receivedBytes);

                Logger.Info($"[BasestationConnected]Accommodating tunnel with id={tunnelId} to the list.");

                lock (openTunnels) {
                    openTunnels.Add(tunnelId, stream);
                }

                // send back ack
                SslListener.SendMessage(stream, CommunicationCodes.ACK);
            }
            catch (Exception ex) {
                Logger.Trace(ex, $"[MobileAppConnected]An error occured (ep={remoteEndPoint}).");
            }
        }

        private void Stop() {
            lock (openTunnels) {
                foreach (var connection in openTunnels) {
                    connection.Value?.Close();
                }

                openTunnels.Clear();
            }
        }
    }
}
