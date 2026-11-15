namespace Source191.Cader.Tui;

/// <summary>What a dialog that asks for typed text answered: the text, or that it was cancelled.</summary>
internal closed record TypedAnswer
{
    public sealed record Typed(string Text) : TypedAnswer;

    /// <summary>Esc or Cancel closed the dialog.</summary>
    public sealed record Cancelled : TypedAnswer;
}