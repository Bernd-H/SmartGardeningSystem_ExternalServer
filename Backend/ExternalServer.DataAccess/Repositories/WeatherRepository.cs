using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExternalServer.Common.Models.Entities;
using ExternalServer.Common.Specifications;
using ExternalServer.Common.Specifications.DataAccess.Repositories;
using ExternalServer.Common.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using OpenWeatherMap;

namespace ExternalServer.DataAccess.Repositories {

    /// <inheritdoc/>
    public class WeatherRepository : IWeatherRepository {

        private static readonly string AppId = "27ceba5613bb90cfe80904078d6f7887";

        private SemaphoreSlim locker = new SemaphoreSlim(1);

        private ILogger Logger;

        private OpenWeatherMapClient openWeatherMapClient;

        public WeatherRepository(ILoggerService logger) {
            Logger = logger.GetLogger<WeatherRepository>();
            openWeatherMapClient = new OpenWeatherMapClient("27ceba5613bb90cfe80904078d6f7887");
        }

        /// <inheritdoc/>
        public async Task<WeatherData> GetWeatherForecastAndHistory(string location) {
            try {
                await locker.WaitAsync();
                Logger.Info($"[GetWeatherForecastAndHistory]Requesting weather data for location={location}.");

                (WeatherData weatherData, string lat, string lon) = await getForecast(location);
                weatherData = await getHistoricalDailyData(lat, lon, weatherData);

                return weatherData ?? new WeatherData();
            }
            finally {
                locker.Release();
            }
        }

        private async Task<WeatherData> getHistoricalDailyData(string lat = "48.43", string lon = "16.12", WeatherData weatherData = null) {
            try {
                using (var client = new HttpClient()) {
                    HttpResponseMessage response = await client.GetAsync($"https://api.openweathermap.org/data/2.5/onecall?lat={lat}&lon={lon}&exclude=current,minutely,hourly,alerts&appid={AppId}&units=metric");
                    response.EnsureSuccessStatusCode();

                    // parse response
                    string json = Encoding.UTF8.GetString(response.Content.ReadAsByteArrayAsync().Result);
                    var jsonData = JObject.Parse(json);
                    var dailyData = jsonData.SelectToken("daily");

                    double time = dailyData[0].SelectToken("dt").Value<double>();
                    float tempMin = dailyData[0].SelectToken("temp").SelectToken("min").Value<float>();
                    float tempMax = dailyData[0].SelectToken("temp").SelectToken("max").Value<float>();
                    float pressure = dailyData[0].SelectToken("pressure").Value<float>();
                    float humidity = dailyData[0].SelectToken("humidity").Value<float>();
                    float windSpeed = dailyData[0].SelectToken("wind_speed").Value<float>();
                    float uvi = dailyData[0].SelectToken("uvi").Value<float>();
                    float precipitation = 0;
                    if (dailyData[0].Contains("rain")) {
                        precipitation = dailyData[0].SelectToken("rain").Value<float>();
                    }

                    if (weatherData == null) {
                        weatherData = new WeatherData();
                    }

                    weatherData.Day = Utils.UnixTimeStampToDateTime(time);
                    weatherData.Humidity = humidity;
                    weatherData.Temp_max = tempMax;
                    weatherData.Temp_min = tempMin;
                    weatherData.MaximumUVIndex = uvi;
                    weatherData.PrecipitationVolume_mm = precipitation;
                    weatherData.Pressure = pressure;
                    weatherData.WindSpeed = windSpeed;

                    return weatherData;
                }
            }
            catch (Exception ex) {
                Logger.Error(ex, $"[getHistoricalDailyData]An error occured while getting weather data for lat={lat}, lon={lon}.");
            }

            return weatherData;
        }

        private async Task<(WeatherData, string, string)> getForecast(string location) {
            try {
                var forecastResponse = await openWeatherMapClient.Forecast.GetByName(location);

                double precipitation = 0;
                WeatherData result = new WeatherData();
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

                var lat = Convert.ToString(forecastResponse.Location.CityLocation.Latitude);
                var lon = Convert.ToString(forecastResponse.Location.CityLocation.Longitude);

                return (result, lat, lon);
            }
            catch (Exception ex) {
                Logger.Error(ex, $"[getForecast]An error occured while getting weather forecast for location={location}.");
            }

            return (null, string.Empty, string.Empty);
        }
    }
}
