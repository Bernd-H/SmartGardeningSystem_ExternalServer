using System.Threading.Tasks;
using ExternalServer.Common.Models.Entities;

namespace ExternalServer.Common.Specifications.DataAccess.Repositories {
    public interface IWeatherRepository {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="location">city name</param>
        /// <returns></returns>
        Task<WeatherForecast> GetCurrentWeatherPredictions(string location);
    }
}
