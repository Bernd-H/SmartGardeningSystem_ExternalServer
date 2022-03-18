using System;

namespace ExternalServer.Common.Models.Entities {
    public class WeatherData {

        #region Forecast

        public DateTime From { get; set; }

        public DateTime To { get; set; }

        public float AmountOfRainInMm { get; set; }

        #endregion

        #region Historical daily data

        public DateTime Day { get; set; }

        public float Temp_min { get; set; }

        public float Temp_max { get; set; }

        public float Pressure { get; set; }

        public float Humidity { get; set; }

        public float WindSpeed { get; set; }

        public float PrecipitationVolume_mm { get; set; }

        public float MaximumUVIndex { get; set; }

        #endregion

        public WeatherData() {
            Temp_min = 0;
            Temp_max = 0;
            Pressure = 0;
            Humidity = 0;
            WindSpeed = 0;
            PrecipitationVolume_mm = 0;
            MaximumUVIndex = 0;
            Day = DateTime.MinValue;
            From = DateTime.MinValue;
            To = DateTime.MinValue;
            AmountOfRainInMm = 0;
        }
    }
}
