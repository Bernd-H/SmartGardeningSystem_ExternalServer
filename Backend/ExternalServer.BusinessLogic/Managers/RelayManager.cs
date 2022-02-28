using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using ExternalServer.Common.Configuration;
using ExternalServer.Common.Events.Communication;
using ExternalServer.Common.Specifications;
using ExternalServer.Common.Specifications.DataAccess.Communication;
using ExternalServer.Common.Specifications.Managers;
using ExternalServer.Common.Utilities;
using Microsoft.Extensions.Configuration;
using NLog;

namespace ExternalServer.BusinessLogic.Managers {

    /// <inheritdoc/>
    public class RelayManager : IRelayManager {

        private ConcurrentDictionary<Guid, SslStream> openTunnels = new ConcurrentDictionary<Guid, SslStream>();


        private ISslListener SslListener;

        private IConfiguration Configuration;

        private ILogger Logger;

        public RelayManager(ILoggerService loggerService, IConfiguration configuration, ISslListener sslListener) {
            Logger = loggerService.GetLogger<RelayManager>();
            Configuration = configuration;
            SslListener = sslListener;
        }

        /// <inheritdoc/>
        public void Start(CancellationToken cancellationToken) {
            cancellationToken.Register(() => Stop());
            int port = Convert.ToInt32(Configuration[ConfigurationVars.BASESTATIONRELAYTUNNEL_PORT]);
            Logger.Info($"[Start]Starting RelayManager.");
            SslListener.ClientConnectedEventHandler += BasestationConnected;
            SslListener.Start(cancellationToken, new IPEndPoint(IPAddress.Any, port), keepAliveInterval: 0, receiveTimeout: System.Threading.Timeout.Infinite);
            //SslListener.Start(cancellationToken, new IPEndPoint(IPAddress.Any, port), BasestationConnected, keepAliveInterval: 0, receiveTimeout: System.Threading.Timeout.Infinite);
        }

        /// <inheritdoc/>
        public SslStream GetTunnelToBasestation(Guid tunnelId) {
            return openTunnels.GetValueOrDefault(tunnelId, defaultValue: null);
        }

        private void BasestationConnected(object o, ClientConnectedEventArgs args) {
            SslStream stream = (SslStream)args.Stream;
            TcpClient tcpClient = args.TcpClient;

            var remoteEndPoint = tcpClient.Client.RemoteEndPoint;
            Logger.Info($"[BasestationConnected]Basestation with IP={remoteEndPoint} connected.");

            try {
                // get the Basestation Id the client want's to connect to
                var receivedBytes = SslListener.ReadMessage(stream);
                var tunnelId = new Guid(receivedBytes);

                Logger.Info($"[BasestationConnected]Accommodating tunnel with id={tunnelId} to the list.");

                openTunnels.AddOrUpdate(tunnelId, stream, (id, existingStream) => {
                    // replace the existing stream with the new stream
                    // this should new happen...
                    existingStream?.Close();
                    return stream;
                });

                // send back ack
                SslListener.SendMessage(stream, CommunicationCodes.ACK);
            }
            catch (Exception ex) {
                Logger.Trace(ex, $"[MobileAppConnected]An error occured (ep={remoteEndPoint}).");
            }
        }

        private void Stop() {
            foreach (var connection in openTunnels.Values) {
                connection?.Close();
            }

            openTunnels.Clear();
        }
    }
}
