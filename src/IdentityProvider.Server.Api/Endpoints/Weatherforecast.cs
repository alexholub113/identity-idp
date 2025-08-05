using MinimalEndpoints.Abstractions;

namespace IdentityProvider.Server.Api.Endpoints;

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public class Weatherforecast : IEndpoint
{
    private readonly static string[] _summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("weatherforecast", Handle);
    }

    private static WeatherForecast[] Handle()
    {
        var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            _summaries[Random.Shared.Next(_summaries.Length)]
        )).ToArray();

        return forecast;
    }
}
