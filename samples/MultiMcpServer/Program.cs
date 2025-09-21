using MultiMcpServer.Tools;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

var builder = WebApplication.CreateBuilder(args);

// Configure the weather server instance
builder.Services.AddMcpServer("weather", options =>
{
    options.ServerInfo = new Implementation
    {
        Name = "WeatherServer",
        Version = "1.0.0"
    };
})
.WithHttpTransport("weather")
.WithTools<WeatherTools>();

// Configure the math server instance
builder.Services.AddMcpServer("math", options =>
{
    options.ServerInfo = new Implementation
    {
        Name = "MathServer",
        Version = "1.0.0"
    };
})
.WithHttpTransport("math")
.WithTools<MathTools>();

// Configure the utility server instance
builder.Services.AddMcpServer("utility", options =>
{
    options.ServerInfo = new Implementation
    {
        Name = "UtilityServer",
        Version = "1.0.0"
    };
})
.WithHttpTransport("utility")
.WithTools<UtilityTools>();

var app = builder.Build();

// Map each MCP server to its own endpoint
app.MapMcp("weather", "/mcp/weather");
app.MapMcp("math", "/mcp/math");
app.MapMcp("utility", "/mcp/utility");

// Add a debug endpoint to test keyed services
app.MapGet("/debug/{serverKey}", (string serverKey, IServiceProvider sp) =>
{
    try
    {
        var keyedTools = sp.GetKeyedServices<McpServerTool>(serverKey).ToList();
        return Results.Ok(new { 
            serverKey, 
            toolCount = keyedTools.Count,
            tools = keyedTools.Select(t => t.ProtocolTool.Name).ToArray()
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(new { 
            serverKey, 
            error = ex.Message
        });
    }
});

// Add a simple info endpoint
app.MapGet("/", () => new
{
    message = "Multi-MCP Server Demo",
    endpoints = new[]
    {
        new { name = "Weather Server", path = "/mcp/weather", tools = new[] { "get_weather", "get_forecast" } },
        new { name = "Math Server", path = "/mcp/math", tools = new[] { "add", "subtract", "multiply", "divide", "power", "sqrt" } },
        new { name = "Utility Server", path = "/mcp/utility", tools = new[] { "echo", "timestamp", "uuid", "encode_base64", "decode_base64" } }
    }
});

app.Run();