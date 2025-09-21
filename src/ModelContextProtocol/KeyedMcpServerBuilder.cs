using ModelContextProtocol;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Keyed implementation of <see cref="IMcpServerBuilder"/> that enables fluent configuration
/// of the Model Context Protocol (MCP) server for a specific server key. This builder is returned by the
/// keyed AddMcpServer extension method and provides access to the service collection for registering additional MCP components
/// while tracking the specific server key for proper service isolation.
/// </summary>
public sealed class KeyedMcpServerBuilder : IMcpServerBuilder
{
    /// <inheritdoc/>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Gets the server key that identifies this specific MCP server instance.
    /// </summary>
    public string ServerKey { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyedMcpServerBuilder"/> class.
    /// </summary>
    /// <param name="services">The service collection to which MCP server services will be added. This collection
    /// is exposed through the <see cref="Services"/> property to allow additional configuration.</param>
    /// <param name="serverKey">The unique key identifying this MCP server instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serverKey"/> is null.</exception>
    public KeyedMcpServerBuilder(IServiceCollection services, string serverKey)
    {
        Throw.IfNull(services);
        Throw.IfNull(serverKey);

        Services = services;
        ServerKey = serverKey;
    }
}
