using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;

namespace ModelContextProtocol.AspNetCore;

/// <summary>
/// Simple options factory that always returns the same named instance.
/// </summary>
/// <typeparam name="TOptions">The options type.</typeparam>
internal sealed class KeyedOptionsFactory<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TOptions>(IOptionsMonitor<TOptions> optionsMonitor, string keyName) : IOptionsFactory<TOptions>
    where TOptions : class
{
    public TOptions Create(string name)
    {
        // Always return our specific named instance regardless of the requested name
        return optionsMonitor.Get(keyName);
    }
}