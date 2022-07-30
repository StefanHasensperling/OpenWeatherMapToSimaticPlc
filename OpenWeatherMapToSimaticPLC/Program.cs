using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using DotNetSiemensPLCToolBoxLibrary.Communication;
using DotNetSiemensPLCToolBoxLibrary.DataTypes;

namespace RefreshWeatherData
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string urlTemplate = "http://api.openweathermap.org/data/2.5/weather?q={1}&APPID={0}&units=metric";
                string url = string.Format(urlTemplate,
                                            Properties.Settings.Default.OpenWeatherMapApiKey,
                                            Properties.Settings.Default.OpenWeatherMapLocationName);

                var json = LoadJsonFromServer(url);
                Console.WriteLine();
                var Weather = ParseWeatherData(json);
                Console.WriteLine();
                SendToPlc(Weather);

                Environment.ExitCode = 0;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(ex.ToString());
                Console.ResetColor();
                Environment.ExitCode = -1;
            }
            finally
            {
                if (System.Diagnostics.Debugger.IsAttached) Console.ReadKey();
            }
        }

        /// <summary>
        /// Connect to OpenWeatherMap, and get the weather data from for the location specified
        /// </summary>
        /// <param name="urlString"></param>
        /// <returns>an Json formatted response</returns>
        private static string LoadJsonFromServer(string urlString)
        {
            string json = "";
            var Uri = new Uri(urlString);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlString);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            Console.WriteLine(String.Format("Sending Web-request: {0}", Uri.Host));
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("Loading data");
                    json = reader.ReadToEnd();
                    Console.WriteLine("Response received");
                }
                else
                {
                    Console.WriteLine(String.Format("Request was acknowledged with {0}", response.StatusCode.ToString()));
                }
            }

            return json;
        }

        /// <summary>
        /// Parse the Json response into an WeatherData
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private static WeatherData ParseWeatherData(string json)
        {
            Console.WriteLine("Parsing response data from server");
            WeatherData Weather = new WeatherData(json);
            foreach (var prop in Weather.GetType().GetProperties())
            {
                Console.WriteLine(String.Format("{0}:\t{1}", prop.Name, prop.GetValue(Weather)));
            }
            
            return Weather;
        }

        /// <summary>
        /// take the WeatherData and send it to the PLC
        /// </summary>
        /// <param name="Weather"></param>
        private static void SendToPlc(WeatherData Weather)
        {
            var s7Cfg = new PLCConnectionConfiguration();
            s7Cfg.ConnectionType = LibNodaveConnectionTypes.ISO_over_TCP;
            s7Cfg.CpuIP = Properties.Settings.Default.PlcIpAddress;
            s7Cfg.CpuSlot = Properties.Settings.Default.PlcSlot;
            s7Cfg.CpuRack = Properties.Settings.Default.PlcRack;

            var tags = new List<PLCTag>();

            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.TemperaturaAddress))
            {
                tags.Add(new PLCTag(Properties.Settings.Default.TemperaturaAddress, TagDataType.Float)
                { Controlvalue = Weather.Temperature });
            }

            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.PressureAddress))
            {
                tags.Add(new PLCTag(Properties.Settings.Default.PressureAddress, TagDataType.Float)
                { Controlvalue = Weather.Pressure });
            }
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.HumidityAddress))
            {
                tags.Add(new PLCTag(Properties.Settings.Default.HumidityAddress, TagDataType.Float)
                { Controlvalue = Weather.Humidity });
            }

            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.WindSpeedAddress))
            {
                tags.Add(new PLCTag(Properties.Settings.Default.WindSpeedAddress, TagDataType.Float)
                { Controlvalue = Weather.WindSpeed });
            }

            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.WindDirectionAddress))
            {
                tags.Add(new PLCTag(Properties.Settings.Default.WindDirectionAddress, TagDataType.Float)
                { Controlvalue = Weather.WindDirection });
            }

            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.CloudPercentAddress))
            {
                tags.Add(new PLCTag(Properties.Settings.Default.CloudPercentAddress, TagDataType.Float)
                { Controlvalue = Weather.CloudPercent });
            }

            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.LocationNameAddress))
            {
                tags.Add(new PLCTag(Properties.Settings.Default.LocationNameAddress, TagDataType.String)
                {
                    ArraySize = Properties.Settings.Default.LocationNameStringLength,
                    Controlvalue = Weather.LocationName
                });
            }

            var s7 = new PLCConnection(s7Cfg);
            Console.WriteLine(string.Format("Connecting to plc: {0}", s7Cfg.CpuIP));
            s7.Connect();

            Console.WriteLine("Write values to plc");
            s7.WriteValues(tags);

            Console.WriteLine("All values written, Disconnecting PLC");
            s7.Disconnect();
        }
    }
}
