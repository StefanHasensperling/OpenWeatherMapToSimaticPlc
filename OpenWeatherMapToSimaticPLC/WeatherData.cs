using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefreshWeatherData
{
    internal class WeatherData
    {
        public double Temperature { get; set; }
        public double Pressure { get; set; }
        public double Humidity { get; set; }
        public string LocationName { get; set; }
        public double WindSpeed { get; set; }
        public double WindDirection { get; set; }
        public double CloudPercent { get; set; }

        public WeatherData()
        {
        }

        public WeatherData(string Json)
        {
            dynamic Weather = JsonConvert.DeserializeObject(Json);
            LocationName = Weather.name;
            Temperature = Weather.main.temp;
            Pressure = Weather.main.pressure;
            Humidity = Weather.main.humidity;
            WindSpeed = Weather.wind.speed;
            WindDirection = Weather.wind.deg;
            CloudPercent = Weather.clouds.all;
        }
    }
}
