namespace Source191.Cader.Application.Views;

/// <summary>The keys that resize the shown tab's detail pane: + grows it by a line and - shrinks it (Shift+= arrives as +).</summary>
public static class PaneKeys
{
    /// <summary>The lines to step by, 1 or -1; false for a key that does not resize the pane.</summary>
    public static bool TryFor(Key key, out int step)
    {
        step = key switch
        {
            { Name: KeyName.Character, Character: '+' } => 1,
            { Name: KeyName.Character, Character: '-' } => -1,
            _ => 0,
        };

        return step != 0;
    }
}