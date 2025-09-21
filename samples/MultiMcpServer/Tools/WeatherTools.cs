using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MultiMcpServer.Tools;

/// <summary>
/// Tools for the weather server instance
/// </summary>
[McpServerToolType]
public class WeatherTools
{
    [McpServerTool(Name = "get_weather"), Description("Get current weather for a city")]
    public static string GetWeather(string city = "London")
    {
        var random = new Random();
        var temperature = random.Next(-10, 35);
        var conditions = new[] { "Sunny", "Cloudy", "Rainy", "Snowy" };
        var condition = conditions[random.Next(conditions.Length)];
        
        return $"Weather in {city}: {temperature}°C, {condition}";
    }

    [McpServerTool(Name = "get_forecast"), Description("Get 5-day weather forecast for a city")]
    public static string GetForecast(string city = "London")
    {
        var random = new Random();
        var forecast = new List<string>();
        
        for (int i = 1; i <= 5; i++)
        {
            var temperature = random.Next(-10, 35);
            var conditions = new[] { "Sunny", "Cloudy", "Rainy", "Snowy" };
            var condition = conditions[random.Next(conditions.Length)];
            forecast.Add($"Day {i}: {temperature}°C, {condition}");
        }
        
        return $"5-day forecast for {city}:\n{string.Join("\n", forecast)}";
    }
}