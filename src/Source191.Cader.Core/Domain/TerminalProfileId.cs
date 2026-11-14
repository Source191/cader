namespace Source191.Cader.Domain;

/// <summary>A Windows Terminal profile's <c>guid</c>.</summary>
public sealed record TerminalProfileId(Guid Value)
{
    public string Text => Value.ToString();

    public static TerminalProfileId ForFragment(string source, string name) => new(Guid.Empty);
}
