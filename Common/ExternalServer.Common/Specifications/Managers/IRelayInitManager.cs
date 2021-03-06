using System.Net;
using System.Threading;

namespace ExternalServer.Common.Specifications.Managers {

    /// <summary>
    /// Starts accepting relay requests from mobile apps
    /// </summary>
    public interface IRelayInitManager {

        void Start(CancellationToken cancellationToken);
    }
}
