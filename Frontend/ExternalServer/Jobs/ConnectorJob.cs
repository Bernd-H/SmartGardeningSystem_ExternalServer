using System;
using System.Threading;
using System.Threading.Tasks;
using ExternalServer.Common.Configuration;
using ExternalServer.Common.Specifications;
using ExternalServer.Common.Specifications.Jobs;
using ExternalServer.Common.Specifications.Managers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NLog;

namespace ExternalServer.Jobs {

    /// <inheritdoc/>
    public class ConnectorJob : IHostedService, IConnectorJob {

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private IConnectionsManager ConnectionsManager;

        private IRelayInitManager RelayInitManager;

        private IRelayManager RelayManager;

        private IConfiguration Configuration;

        private ILogger Logger;

        public ConnectorJob(ILoggerService loggerService, IConnectionsManager connectionsManager, IConfiguration configuration, IRelayInitManager relayInitManager,
            IRelayManager relayManager) {
            Logger = loggerService.GetLogger<ConnectorJob>();
            ConnectionsManager = connectionsManager;
            Configuration = configuration;
            RelayInitManager = relayInitManager;
            RelayManager = relayManager;
        }

        /// <inheritdoc/>
        public Task StartAsync(CancellationToken cancellationToken) {
            Logger.Info($"[StartAsync]Starting ConnectionsManager.");

            ConnectionsManager.Start(_cancellationTokenSource.Token);
            RelayInitManager.Start(_cancellationTokenSource.Token);
            RelayManager.Start(_cancellationTokenSource.Token);

            Task.Run(async () => {
                // clean up "ghost" connections in a specific interval
                while (true) {
                    float interval = Convert.ToSingle(Configuration[ConfigurationVars.CONNECTORJOB_CLEANUPINTERVAL_H]) * 60 * 60 * 1000;
                    await Task.Delay(Convert.ToInt32(interval));

                    ConnectionsManager.CleanupGhostConenctions();
                }
            }, _cancellationTokenSource.Token);

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task StopAsync(CancellationToken cancellationToken) {
            Logger.Info($"[StopAsync]Closing all open connections.");

            _cancellationTokenSource.Cancel();

            return Task.CompletedTask;
        }
    }
}
