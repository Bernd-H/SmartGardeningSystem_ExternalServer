using NLog;

namespace ExternalServer.Common.Specifications {
    public interface ILoggerService {
        ILogger GetLogger<T>() where T : class;
    }
}
