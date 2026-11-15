namespace Source191.Cader.Application.Views;

/// <summary>A key as the reducer sees it, independent of any terminal library.</summary>
public sealed record Key(KeyName Name, char Character)
{
    public static Key Enter => new(KeyName.Enter, '\0');
    public static Key Escape => new(KeyName.Escape, '\0');
    public static Key Up => new(KeyName.Up, '\0');
    public static Key Down => new(KeyName.Down, '\0');
    public static Key Left => new(KeyName.Left, '\0');
    public static Key Right => new(KeyName.Right, '\0');
    public static Key Backspace => new(KeyName.Backspace, '\0');
    public static Key Slash => new(KeyName.Character, '/');

    public static Key Of(char character) => new(KeyName.Character, character);
}