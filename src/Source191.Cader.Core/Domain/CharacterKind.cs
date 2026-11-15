namespace Source191.Cader.Domain;

/// <summary>What kind of character a person typed, or a terminal sent: the only fact about it the log may hold.</summary>
public enum CharacterKind
{
    LetterOrDigit,
    Whitespace,
    Control,
    Format,
    Other,
}