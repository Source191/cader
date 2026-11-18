using System.Diagnostics.CodeAnalysis;

namespace Source191.Cader.Application.Views;

/// <summary>The key every tab shares for refreshing now: F5, answered ahead of filter typing so it leaves a filter being typed alone.</summary>
public static class RefreshKeys
{
    /// <summary>The effect of the refresh key; false for a key that is not it.</summary>
    public static bool TryFor(Key key, [MaybeNullWhen(false)] out ViewEffect effect)
    {
        effect = key.Name == KeyName.F5 ? new RefreshShownTab() : null;

        return effect is not null;
    }
}