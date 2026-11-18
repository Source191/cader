namespace Source191.Cader.Domain;

public sealed record RowFilter(string Text)
{
    public static readonly RowFilter None = new(string.Empty);

    public bool Matches(IReadOnlyList<string> fields)
    {
        string visible = CharacterKinds.WithoutInvisibleStart(Text);

        if (visible.Length == 0)
        {
            return true;
        }

        return fields.Any(field => field.Contains(visible, StringComparison.OrdinalIgnoreCase));
    }
}