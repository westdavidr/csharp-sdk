using MultiMcpServer.Tools;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

var builder = WebApplication.CreateBuilder(args);

// Configure the weather server instance
builder.Services.AddMcpServer("weather")
    .WithHttpTransport("weather")
    .WithTools<WeatherTools>();

// Configure the math server instance
builder.Services.AddMcpServer("math")
    .WithHttpTransport("math")
    .WithTools<MathTools>();

// Configure the utility server instance
builder.Services.AddMcpServer("utility")
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
        return Results.Ok(new
        {
            serverKey,
            toolCount = keyedTools.Count,
            tools = keyedTools.Select(t => t.ProtocolTool.Name).ToArray()
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(new
        {
            serverKey,
            error = ex.Message
        });
    }
});

app.Run();