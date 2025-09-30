using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol;
using ModelContextProtocol.AspNetCore;
using ModelContextProtocol.Server;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides methods for configuring HTTP MCP servers via dependency injection.
/// </summary>
public static class HttpMcpServerBuilderExtensions
{
    /// <summary>
    /// Adds the services necessary for <see cref="M:McpEndpointRouteBuilderExtensions.MapMcp"/>
    /// to handle MCP requests and sessions using the MCP Streamable HTTP transport. For more information on configuring the underlying HTTP server
    /// to control things like port binding custom TLS certificates, see the <see href="https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis">Minimal APIs quick reference</see>.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="configureOptions">Configures options for the Streamable HTTP transport. This allows configuring per-session
    /// <see cref="McpServerOptions"/> and running logic before and after a session.</param>
    /// <returns>The builder provided in <paramref name="builder"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
    public static IMcpServerBuilder WithHttpTransport(this IMcpServerBuilder builder, Action<HttpServerTransportOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.TryAddSingleton<StatefulSessionManager>();
        builder.Services.TryAddSingleton<StreamableHttpHandler>();
        builder.Services.TryAddSingleton<SseHandler>();
        builder.Services.AddHostedService<IdleTrackingBackgroundService>();
        builder.Services.AddDataProtection();

        builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IPostConfigureOptions<McpServerOptions>, AuthorizationFilterSetup>());

        if (configureOptions is not null)
        {
            builder.Services.Configure(configureOptions);
        }

        return builder;
    }

    /// <summary>
    /// Adds the services necessary for a keyed MCP server instance that can be mapped with <see cref="M:McpEndpointRouteBuilderExtensions.MapMcp(IEndpointRouteBuilder, string, string)"/>
    /// to handle MCP requests and sessions using the MCP Streamable HTTP transport.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="serverKey">The unique key for this MCP server instance.</param>
    /// <param name="configureOptions">Configures options for the Streamable HTTP transport. This allows configuring per-session
    /// <see cref="McpServerOptions"/> and running logic before and after a session.</param>
    /// <returns>The builder provided in <paramref name="builder"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="serverKey"/> is <see langword="null"/>.</exception>
    public static IMcpServerBuilder WithHttpTransport(this IMcpServerBuilder builder, string serverKey, Action<HttpServerTransportOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(serverKey);

        // Ensure options services are registered for keyed scenarios
        builder.Services.AddOptions();

        // Register authorization filter setup for all options (only once globally)
        // This should be done once for the application, not per server
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<McpServerOptions>, AuthorizationFilterSetup>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<McpServerOptions>, AuthorizationFilterSetup>());

        // Register keyed options configuration using standard .NET options pattern
        // We need to create a unique instance per server key to avoid duplicate registration issues
        builder.Services.AddSingleton<IConfigureNamedOptions<McpServerOptions>>(sp =>
            new KeyedMcpServerOptionsSetup(sp, serverKey, sp.GetRequiredService<IOptions<McpServerHandlers>>()));

        // Register keyed options monitor access
        builder.Services.TryAddKeyedSingleton(serverKey, (sp, key) =>
        {
            var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<McpServerOptions>>();
            return Options.Options.Create(optionsMonitor.Get((string)key!));
        });

        // Register keyed options factory
        builder.Services.TryAddKeyedSingleton<IOptionsFactory<McpServerOptions>>(serverKey, (sp, key) =>
        {
            var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<McpServerOptions>>();
            return new KeyedOptionsFactory<McpServerOptions>(optionsMonitor, (string)key!);
        });

        // For HttpServerTransportOptions, we can use standard named options
        builder.Services.TryAddKeyedSingleton(serverKey, (sp, key) =>
        {
            var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<HttpServerTransportOptions>>();
            return Options.Options.Create(optionsMonitor.Get((string)key!));
        });

        // Register keyed services for this specific server instance
        builder.Services.TryAddKeyedSingleton(serverKey, (sp, key) => 
            new StatefulSessionManager(
                sp.GetRequiredKeyedService<IOptions<HttpServerTransportOptions>>(key),
                sp.GetRequiredService<ILogger<StatefulSessionManager>>()));

        builder.Services.TryAddKeyedSingleton(serverKey, (sp, key) =>
            new StreamableHttpHandler(
                sp.GetRequiredKeyedService<IOptions<McpServerOptions>>(key),
                sp.GetRequiredKeyedService<IOptionsFactory<McpServerOptions>>(key),
                sp.GetRequiredKeyedService<IOptions<HttpServerTransportOptions>>(key),
                sp.GetRequiredKeyedService<StatefulSessionManager>(key),
                sp.GetRequiredService<IDataProtectionProvider>(),
                sp.GetRequiredService<ILoggerFactory>(),
                sp));

        builder.Services.TryAddKeyedSingleton(serverKey, (sp, key) =>
            new SseHandler(
                sp.GetRequiredKeyedService<IOptions<McpServerOptions>>(key),
                sp.GetRequiredKeyedService<IOptionsFactory<McpServerOptions>>(key),
                sp.GetRequiredKeyedService<IOptions<HttpServerTransportOptions>>(key),
                sp.GetRequiredService<IHostApplicationLifetime>(),
                sp.GetRequiredService<ILoggerFactory>()));

        // Register a keyed background service for this server instance
        builder.Services.TryAddKeyedSingleton(serverKey, (sp, key) =>
            new IdleTrackingBackgroundService(
                sp.GetRequiredKeyedService<StatefulSessionManager>(key),
                sp.GetRequiredKeyedService<IOptions<HttpServerTransportOptions>>(key),
                sp.GetRequiredService<IHostApplicationLifetime>(),
                sp.GetRequiredService<ILogger<IdleTrackingBackgroundService>>()));

        // Register as hosted service so it gets started by the host
        builder.Services.AddSingleton<IHostedService>(sp => sp.GetRequiredKeyedService<IdleTrackingBackgroundService>(serverKey));

        // Add data protection (shared across all instances)
        builder.Services.AddDataProtection();

        if (configureOptions is not null)
        {
            builder.Services.Configure(serverKey, configureOptions);
        }

        return builder;
    }

    /// <summary>
    /// Adds authorization filters to support <see cref="AuthorizeAttribute"/>
    /// on MCP server tools, prompts, and resources. This method should always be called when using
    /// ASP.NET Core integration to ensure proper authorization support.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <returns>The builder provided in <paramref name="builder"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This method automatically configures authorization filters for all MCP server handlers. These filters respect
    /// authorization attributes such as <see cref="AuthorizeAttribute"/>
    /// and <see cref="AllowAnonymousAttribute"/>.
    /// </remarks>
    public static IMcpServerBuilder AddAuthorizationFilters(this IMcpServerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Allow the authorization filters to get added multiple times in case other middleware changes the matched primitive.
        builder.Services.AddTransient<IConfigureOptions<McpServerOptions>, AuthorizationFilterSetup>();

        return builder;
    }
}
