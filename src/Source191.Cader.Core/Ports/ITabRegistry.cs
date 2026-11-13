using Source191.Cader.Domain;

namespace Source191.Cader.Ports;

/// <summary>The tabs the console has opened, one record each, read with stale ones still present (the caller judges liveness).</summary>
public interface ITabRegistry
{
    Task<IReadOnlyList<TabRegistration>> ReadTabsAsync();

    Task WriteTabAsync(TabRegistration registration);

    Task RemoveTabAsync(Guid id);
}