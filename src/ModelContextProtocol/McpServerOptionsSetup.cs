using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

namespace ModelContextProtocol;

/// <summary>
/// Configures the McpServerOptions using addition services from DI.
/// </summary>
/// <param name="serverHandlers">The server handlers configuration options.</param>
/// <param name="serverTools">Tools individually registered.</param>
/// <param name="serverPrompts">Prompts individually registered.</param>
/// <param name="serverResources">Resources individually registered.</param>
internal sealed class McpServerOptionsSetup(
    IOptions<McpServerHandlers> serverHandlers,
    IEnumerable<McpServerTool> serverTools,
    IEnumerable<McpServerPrompt> serverPrompts,
    IEnumerable<McpServerResource> serverResources) : IConfigureOptions<McpServerOptions>
{
    /// <summary>
    /// Configures the given McpServerOptions instance by setting server information
    /// and applying custom server handlers and tools.
    /// </summary>
    /// <param name="options">The options instance to be configured.</param>
    public void Configure(McpServerOptions options)
    {
        Throw.IfNull(options);

        // Use the shared helper to configure capabilities
        McpServerOptionsHelper.ConfigureCapabilities(options, serverTools, serverPrompts, serverResources);

        // Apply custom server handlers
        serverHandlers.Value.OverwriteWithSetHandlers(options);
    }
}
