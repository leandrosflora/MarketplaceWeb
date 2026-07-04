using Microsoft.Extensions.Options;

namespace MarketplaceWeb.Tests.Auth;

internal sealed class StaticOptionsMonitor<T> : IOptionsMonitor<T>
{
    public StaticOptionsMonitor(T value)
    {
        CurrentValue = value;
    }

    public T CurrentValue { get; }

    public T Get(string? name) => CurrentValue;

    public IDisposable? OnChange(Action<T, string?> listener) => null;
}
