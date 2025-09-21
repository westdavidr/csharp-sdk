using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// A wrapper that implements <see cref="IOptions{T}"/> and <see cref="IOptionsFactory{T}"/> for keyed services.
/// This class provides named options support for keyed MCP server instances by manually configuring
/// options with the appropriate keyed services (tools, prompts, resources) since the standard .NET options
/// pattern doesn't automatically work with keyed dependency injection.
/// </summary>
/// <typeparam name="T">The options type</typeparam>
internal sealed class KeyedOptionsWrapper<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T> : IOptions<T>, IOptionsFactory<T> where T : class
{
    private readonly IOptionsMonitor<T> _optionsMonitor;
    private readonly string _name;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyedOptionsWrapper{T}"/> class.
    /// </summary>
    /// <param name="optionsMonitor">The options monitor to retrieve base options from.</param>
    /// <param name="name">The name/key of the options instance.</param>
    /// <param name="serviceProvider">The service provider to resolve keyed services from.</param>
    public KeyedOptionsWrapper(IOptionsMonitor<T> optionsMonitor, string name, IServiceProvider serviceProvider)
    {
        _optionsMonitor = optionsMonitor;
        _name = name;
        _serviceProvider = serviceProvider;
        
        // Get the base options first
        Value = _optionsMonitor.Get(name);
        
        // If this is McpServerOptions, manually configure it with keyed services
        if (typeof(T) == typeof(McpServerOptions) && Value is McpServerOptions mcpOptions)
        {
            // Get keyed services for this specific server instance
            var serverTools = _serviceProvider.GetKeyedServices<McpServerTool>(name);
            var serverPrompts = _serviceProvider.GetKeyedServices<McpServerPrompt>(name);
            var serverResources = _serviceProvider.GetKeyedServices<McpServerResource>(name);

            // Configure tools
            var toolsList = serverTools.ToList();
            if (toolsList.Count > 0)
            {
                McpServerPrimitiveCollection<McpServerTool> toolCollection = mcpOptions.Capabilities?.Tools?.ToolCollection ?? [];
                foreach (var tool in toolsList)
                {
                    toolCollection.TryAdd(tool);
                }

                mcpOptions.Capabilities ??= new();
                mcpOptions.Capabilities.Tools ??= new();
                mcpOptions.Capabilities.Tools.ToolCollection = toolCollection;
            }

            // Configure prompts
            var promptsList = serverPrompts.ToList();
            if (promptsList.Count > 0)
            {
                McpServerPrimitiveCollection<McpServerPrompt> promptCollection = mcpOptions.Capabilities?.Prompts?.PromptCollection ?? [];
                foreach (var prompt in promptsList)
                {
                    promptCollection.TryAdd(prompt);
                }

                mcpOptions.Capabilities ??= new();
                mcpOptions.Capabilities.Prompts ??= new();
                mcpOptions.Capabilities.Prompts.PromptCollection = promptCollection;
            }

            // Configure resources
            var resourcesList = serverResources.ToList();
            if (resourcesList.Count > 0)
            {
                McpServerResourceCollection resourceCollection = mcpOptions.Capabilities?.Resources?.ResourceCollection ?? [];
                foreach (var resource in resourcesList)
                {
                    resourceCollection.TryAdd(resource);
                }

                mcpOptions.Capabilities ??= new();
                mcpOptions.Capabilities.Resources ??= new();
                mcpOptions.Capabilities.Resources.ResourceCollection = resourceCollection;
            }
        }
    }

    /// <summary>
    /// Gets the configured options value for this keyed instance.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Creates options for the specified name.
    /// </summary>
    /// <param name="name">The name of the options to create.</param>
    /// <returns>The options instance.</returns>
    public T Create(string name)
    {
        return _optionsMonitor.Get(name);
    }
}
