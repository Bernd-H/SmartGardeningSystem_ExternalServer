﻿using System;

namespace ExternalServer.Common.Models.Entities {
    public class WeatherForecast {

        public DateTime From { get; set; }

        public DateTime To { get; set; }

        public float AmountOfRainInMm { get; set; }
    }
}
