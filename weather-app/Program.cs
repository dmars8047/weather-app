using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace weather_app
{
    class Program
    {
        const string API_Key = "d1d0eba6fd4e13c1be3b89228e0184bb";
        const string Default_Postal_Code = "19053";
        const string Default_Country = "us";
        const string Default_Units = "Metric";

        static void Main(string[] args)
        {
            string postalcode = Default_Postal_Code;
            string country = Default_Country;
            string units = Default_Units;
            bool verbose = false;

            //Look at args
            if (args != null && args.Length > 0)
            {
                if (args.Any(x => x == "--help"))
                {
                    OutputHelpMessageAndExit();
                }


                if (args.Any(x => x == "-p"))
                {
                    var index = Array.IndexOf(args, "-p");

                    try
                    {
                        postalcode = args[index + 1];
                    }
                    catch (Exception)
                    {
                        DisplayErrorAndExit();
                    }
                }

                if (args.Any(x => x == "-c"))
                {
                    var index = Array.IndexOf(args, "-c");

                    try
                    {
                        country = args[index + 1];
                    }
                    catch (Exception)
                    {
                        DisplayErrorAndExit();
                    }
                }

                if (args.Any(x => x == "-u"))
                {
                    var index = Array.IndexOf(args, "-u");

                    try
                    {
                        units = args[index + 1];
                    }
                    catch (Exception)
                    {
                        DisplayErrorAndExit();
                    }
                }

                if (args.Any(x => x == "-v"))
                {
                    var index = Array.IndexOf(args, "-v");

                    verbose = true;
                }
            }

            using (var httpClient = new HttpClient())
            {
                // httpClient.BaseAddress = new Uri("http://api.openweathermap.org/data/2.5/weather");
                var result = httpClient.GetAsync($"http://api.openweathermap.org/data/2.5/weather?zip={postalcode},{country}&units={units}&appid={API_Key}").Result;

                if (result.IsSuccessStatusCode)
                {
                    WeatherResponseObj resultObj = JsonConvert.DeserializeObject<WeatherResponseObj>(result.Content.ReadAsStringAsync().Result);
                    string unitTempDescription = "";

                    switch (units.ToLower())
                    {
                        case "imperial":
                            unitTempDescription = "Ferenheit";
                            break;
                        case "metric":
                            unitTempDescription = "Celcius";
                            break;
                        case "standard":
                            unitTempDescription = "Kelvin";
                            break;
                        default:
                            unitTempDescription = "Celcius";
                            break;
                    };


                    OutputResultsAndExit(resultObj, verbose, unitTempDescription);
                }
                else
                {
                    if (result.StatusCode == HttpStatusCode.BadRequest || result.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        WeatherResponseObj error = JsonConvert.DeserializeObject<WeatherResponseObj>(result.Content.ReadAsStringAsync().Result);

                        DisplayErrorAndExit($"The Weather API returned the following error: {error.Message}");
                    }
                }
            }
        }

        static void DisplayErrorAndExit(string message = null)
        {
            if (String.IsNullOrWhiteSpace(message))
            {
                Console.WriteLine("Uh-oh, something went wrong :(. Exiting program");
            }
            else
            {
                Console.WriteLine(message);
            }

            Environment.Exit(0);
        }

        static void OutputHelpMessageAndExit()
        {
            Console.WriteLine("Welecome to the helpful weather app. Here are some options for weather retrieval: ");
            Console.WriteLine($"-p, Refers to the postal code used for the search. This option should be followed by the value to be used in the search query. Default value is '{Default_Postal_Code}'.");
            Console.WriteLine($"-c, Refers to the country used for the search. This option should be followed by the value to be used in the search query. Default value is '{Default_Country}'.");
            Console.WriteLine($"-u, Refers to the units displayed in results. This option should be followed by the value to be used in the search query. Valid values include: metric, imperial, standard. Default value is '{Default_Units}'.");
            Environment.Exit(0);
        }

        static void OutputResultsAndExit(WeatherResponseObj obj, bool verbose, string unitTempDescription)
        {
            if (!verbose)
            {
                Console.WriteLine($"\nThe current temperature in {obj.Name} is {obj.Main.Temp} degrees ({unitTempDescription}). Current conditions are reported as: '{obj.Weather[0].Description} ({obj.Weather[0].Main})'.\n");
            }
            else
            {
                Console.WriteLine($"\nLocation Name: {obj.Name}");
                Console.WriteLine($"Current Temperature: {obj.Main.Temp} degrees ({unitTempDescription})");
                Console.WriteLine($"Feels Like: {obj.Main.Feels_Like} degrees ({unitTempDescription})");
                Console.WriteLine($"Min Tempurature: {obj.Main.Temp_Min} degrees ({unitTempDescription})");
                Console.WriteLine($"Max Tempurature: {obj.Main.Temp_Max} degrees ({unitTempDescription})");
                Console.WriteLine($"Reported Conditions: {obj.Weather[0].Description} ({obj.Weather[0].Main})'");
                Console.WriteLine($"Sunrise: {UnixTimeStampToDateTime(obj.Sys.Sunrise)}");
                Console.WriteLine($"Sunset: {UnixTimeStampToDateTime(obj.Sys.Sunset)}");
                Console.WriteLine($"Wind Speed: {obj.Wind.Speed}\n");
            }

            Environment.Exit(0);
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
    }

    //     {
    //   "coord": {
    //     "lon": -122.08,
    //     "lat": 37.39
    //   },
    //   "weather": [
    //     {
    //       "id": 800,
    //       "main": "Clear",
    //       "description": "clear sky",
    //       "icon": "01d"
    //     }
    //   ],
    //   "base": "stations",
    //   "main": {
    //     "temp": 282.55,
    //     "feels_like": 281.86,
    //     "temp_min": 280.37,
    //     "temp_max": 284.26,
    //     "pressure": 1023,
    //     "humidity": 100
    //   },
    //   "visibility": 16093,
    //   "wind": {
    //     "speed": 1.5,
    //     "deg": 350
    //   },
    //   "clouds": {
    //     "all": 1
    //   },
    //   "dt": 1560350645,
    //   "sys": {
    //     "type": 1,
    //     "id": 5122,
    //     "message": 0.0139,
    //     "country": "US",
    //     "sunrise": 1560343627,
    //     "sunset": 1560396563
    //   },
    //   "timezone": -25200,
    //   "id": 420006353,
    //   "name": "Mountain View",
    //   "cod": 200
    //   }      

    public class WeatherResponseObj
    {
        public string Message { get; set; }
        public Weather[] Weather { get; set; }
        public Main Main { get; set; }
        public int Visibility { get; set; }
        public Sys Sys { get; set; }
        public Wind Wind { get; set; }
        public string Name { get; set; }
    }

    public class Weather
    {
        public string Main { get; set; }
        public string Description { get; set; }
    }

    public class Wind
    {
        public double Speed { get; set; }
        public int Deg { get; set; }
    }

    public class Sys
    {
        public int Type { get; set; }
        public int Id { get; set; }
        public double Message { get; set; }
        public int Sunrise { get; set; }
        public int Sunset { get; set; }
    }

    public class Main
    {
        public string Temp { get; set; }
        public string Feels_Like { get; set; }
        public string Temp_Min { get; set; }
        public string Temp_Max { get; set; }
        public string Pressure { get; set; }
        public string Humidity { get; set; }
    }
}
