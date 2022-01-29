using System.Threading.Tasks;
using ExternalServer.Common.Models.Entities;

namespace ExternalServer.Common.Specifications.DataAccess.Repositories {

    /// <summary>
    /// Loads and caches weather forecasts.
    /// </summary>
    public interface IWeatherRepository {

        /// <summary>
        /// Gets a weather forecase for a specific location.
        /// </summary>
        /// <param name="location">Name of a city.</param>
        /// <returns>A task that represents an asynchronous operation. The value of the TResult
        /// parameter contains a WeatherForecast object.</returns>
        Task<WeatherForecast> GetCurrentWeatherPredictions(string location);
    }
}
