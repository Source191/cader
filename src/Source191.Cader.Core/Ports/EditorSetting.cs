namespace Source191.Cader.Ports;

/// <summary>The editor as config.json holds it: <c>{ "preset": "VsCode" }</c>, or a custom executable and its argument list; every member is optional.</summary>
public sealed record EditorSetting(string? Preset = null, string? Executable = null, IReadOnlyList<string>? Arguments = null)
{
    public bool Equals(EditorSetting? other)
    {
        return other is not null
            && string.Equals(Preset, other.Preset, StringComparison.Ordinal)
            && string.Equals(Executable, other.Executable, StringComparison.Ordinal)
            && (Arguments ?? []).SequenceEqual(other.Arguments ?? []);
    }

    public override int GetHashCode() => HashCode.Combine(Preset, Executable);
}