using ModelContextProtocol.Server;

namespace ModelContextProtocol;

/// <summary>
/// Shared helper methods for configuring McpServerOptions with services.
/// </summary>
public static class McpServerOptionsHelper
{
    /// <summary>
    /// Configures the given McpServerOptions instance with tools, prompts, and resources.
    /// </summary>
    /// <param name="options">The options instance to be configured.</param>
    /// <param name="serverTools">Tools to add to the options.</param>
    /// <param name="serverPrompts">Prompts to add to the options.</param>
    /// <param name="serverResources">Resources to add to the options.</param>
    public static void ConfigureCapabilities(
        McpServerOptions options,
        IEnumerable<McpServerTool> serverTools,
        IEnumerable<McpServerPrompt> serverPrompts,
        IEnumerable<McpServerResource> serverResources)
    {
        Throw.IfNull(options);

        // Configure tools
        ConfigureToolsCapability(options, serverTools);

        // Configure prompts  
        ConfigurePromptsCapability(options, serverPrompts);

        // Configure resources
        ConfigureResourcesCapability(options, serverResources);
    }

    private static void ConfigureToolsCapability(McpServerOptions options, IEnumerable<McpServerTool> serverTools)
    {
        // Collect all of the provided tools into a tools collection. If the options already has
        // a collection, add to it, otherwise create a new one. We want to maintain the identity
        // of an existing collection in case someone has provided their own derived type, wants
        // change notifications, etc.
        McpServerPrimitiveCollection<McpServerTool> toolCollection = options.Capabilities?.Tools?.ToolCollection ?? [];
        foreach (var tool in serverTools)
        {
            toolCollection.TryAdd(tool);
        }

        if (!toolCollection.IsEmpty)
        {
            options.Capabilities ??= new();
            options.Capabilities.Tools ??= new();
            options.Capabilities.Tools.ToolCollection = toolCollection;
        }
    }

    private static void ConfigurePromptsCapability(McpServerOptions options, IEnumerable<McpServerPrompt> serverPrompts)
    {
        // Collect all of the provided prompts into a prompts collection. If the options already has
        // a collection, add to it, otherwise create a new one. We want to maintain the identity
        // of an existing collection in case someone has provided their own derived type, wants
        // change notifications, etc.
        McpServerPrimitiveCollection<McpServerPrompt> promptCollection = options.Capabilities?.Prompts?.PromptCollection ?? [];
        foreach (var prompt in serverPrompts)
        {
            promptCollection.TryAdd(prompt);
        }

        if (!promptCollection.IsEmpty)
        {
            options.Capabilities ??= new();
            options.Capabilities.Prompts ??= new();
            options.Capabilities.Prompts.PromptCollection = promptCollection;
        }
    }

    private static void ConfigureResourcesCapability(McpServerOptions options, IEnumerable<McpServerResource> serverResources)
    {
        // Collect all of the provided resources into a resources collection. If the options already has
        // a collection, add to it, otherwise create a new one. We want to maintain the identity
        // of an existing collection in case someone has provided their own derived type, wants
        // change notifications, etc.
        McpServerResourceCollection resourceCollection = options.Capabilities?.Resources?.ResourceCollection ?? [];
        foreach (var resource in serverResources)
        {
            resourceCollection.TryAdd(resource);
        }

        if (!resourceCollection.IsEmpty)
        {
            options.Capabilities ??= new();
            options.Capabilities.Resources ??= new();
            options.Capabilities.Resources.ResourceCollection = resourceCollection;
        }
    }
}