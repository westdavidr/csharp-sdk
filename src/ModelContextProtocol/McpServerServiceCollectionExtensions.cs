using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring MCP servers with dependency injection.
/// </summary>
public static class McpServerServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Model Context Protocol (MCP) server to the service collection with default options.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the server to.</param>
    /// <param name="configureOptions">Optional callback to configure the <see cref="McpServerOptions"/>.</param>
    /// <returns>An <see cref="IMcpServerBuilder"/> that can be used to further configure the MCP server.</returns>

    public static IMcpServerBuilder AddMcpServer(this IServiceCollection services, Action<McpServerOptions>? configureOptions = null)
    {
        services.AddOptions();
        services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<McpServerOptions>, McpServerOptionsSetup>());
        if (configureOptions is not null)
        {
            services.Configure(configureOptions);
        }

        return new DefaultMcpServerBuilder(services);
    }

    /// <summary>
    /// Adds a keyed Model Context Protocol (MCP) server to the service collection with the specified server key.
    /// This allows multiple MCP server instances with different configurations to be registered.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the server to.</param>
    /// <param name="serverKey">The unique key for this MCP server instance.</param>
    /// <param name="configureOptions">Optional callback to configure the <see cref="McpServerOptions"/>.</param>
    /// <returns>An <see cref="IMcpServerBuilder"/> that can be used to further configure the MCP server.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="serverKey"/> is <see langword="null"/>.</exception>
    public static IMcpServerBuilder AddMcpServer(this IServiceCollection services, string serverKey, Action<McpServerOptions>? configureOptions = null)
    {
                Throw.IfNull(serverKey);
        
        services.AddOptions();
        if (configureOptions is not null)
        {
            services.Configure<McpServerOptions>(serverKey, configureOptions);
        }

        return new KeyedMcpServerBuilder(services, serverKey);
    }
}
