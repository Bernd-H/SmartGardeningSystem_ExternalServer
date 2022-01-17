using System;
using System.Threading;
using System.Threading.Tasks;
using ExternalServer.Common.Models.Entities;
using ExternalServer.Common.Specifications;
using ExternalServer.Common.Specifications.DataAccess.Repositories;
using NLog;
using OpenWeatherMap;

namespace ExternalServer.DataAccess.Repositories {
    public class WeatherRepository : IWeatherRepository {

        private SemaphoreSlim locker = new SemaphoreSlim(1);

        private ILogger Logger;

        private OpenWeatherMapClient openWeatherMapClient;

        public WeatherRepository(ILoggerService logger) {
            Logger = logger.GetLogger<WeatherRepository>();
            openWeatherMapClient = new OpenWeatherMapClient("27ceba5613bb90cfe80904078d6f7887");
        }

        public async Task<WeatherForecast> GetCurrentWeatherPredictions(string location) {
            try {
                await locker.WaitAsync(); // lock, because no information if openWeatherMapClient is threadsafe.
                Logger.Info($"[GetCurrentWeatherPredictions]Requesting weather predictions for location={location}.");

                var forecastResponse = await openWeatherMapClient.Forecast.GetByName(location);

                double precipitation = 0;
                WeatherForecast result = new WeatherForecast();
                result.From = forecastResponse.Forecast[0].From;
                var oneDayFromNow = DateTime.Now.AddDays(1);

                // look one day ahead
                for (int i = 0; i < forecastResponse.Forecast.Length; i++) {
                    if ((oneDayFromNow - forecastResponse.Forecast[i].To).TotalHours < 0 || i == forecastResponse.Forecast.Length) {
                        result.To = forecastResponse.Forecast[i - 1].To;
                        break;
                    }

                    //Console.WriteLine(forecastResponse.Forecast[i].To.ToString());
                    //Console.WriteLine(forecastResponse.Forecast[i].Precipitation.Type); // rain or snow
                    //Console.WriteLine(forecastResponse.Forecast[i].Precipitation.Unit); // 3h or 1h
                    //Console.WriteLine(forecastResponse.Forecast[i].Precipitation.Value); // for example 1,43 mm
                    if (forecastResponse.Forecast[i]?.Precipitation?.Type?.Equals("rain") ?? false) {
                        precipitation += forecastResponse.Forecast[i].Precipitation.Value;
                    }
                }

                result.AmountOfRainInMm = Convert.ToSingle(precipitation);
                return result;
            }
            catch (Exception ex) {
                Logger.Error(ex, "[GetCurrentWeatherPredictions]Error while getting weahter forecasts.");
            }
            finally {
                locker.Release();
            }

            return null;
        }
    }
}
