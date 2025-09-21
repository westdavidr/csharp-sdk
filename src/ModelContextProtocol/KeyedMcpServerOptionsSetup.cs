using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

namespace ModelContextProtocol;

/// <summary>
/// Configures named McpServerOptions using keyed services from DI.
/// </summary>
/// <param name="serviceProvider">The service provider for resolving keyed services.</param>
/// <param name="serverKey">The key to use for resolving keyed services.</param>
/// <param name="serverHandlers">The server handlers configuration options.</param>
public sealed class KeyedMcpServerOptionsSetup(
    IServiceProvider serviceProvider, 
    string serverKey,
    IOptions<McpServerHandlers> serverHandlers) : IConfigureNamedOptions<McpServerOptions>
{
    /// <summary>
    /// Configures the given McpServerOptions instance by setting server information
    /// and applying custom server handlers and keyed tools/prompts/resources.
    /// </summary>
    /// <param name="name">The name of the options instance being configured.</param>
    /// <param name="options">The options instance to be configured.</param>
    public void Configure(string? name, McpServerOptions options)
    {
        // Only configure if this is for our server key
        if (name != serverKey)
        {
            return;
        }

        Configure(options);
    }

    /// <summary>
    /// Configures the default McpServerOptions instance.
    /// </summary>
    /// <param name="options">The options instance to be configured.</param>
    public void Configure(McpServerOptions options)
    {
        Throw.IfNull(options);

        // Get keyed services for this server instance
        var serverTools = serviceProvider.GetKeyedServices<McpServerTool>(serverKey);
        var serverPrompts = serviceProvider.GetKeyedServices<McpServerPrompt>(serverKey);
        var serverResources = serviceProvider.GetKeyedServices<McpServerResource>(serverKey);

        // Use the shared helper to configure capabilities
        McpServerOptionsHelper.ConfigureCapabilities(options, serverTools, serverPrompts, serverResources);

        // Apply custom server handlers
        serverHandlers.Value.OverwriteWithSetHandlers(options);
    }
}