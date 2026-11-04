namespace Source191.Vantage.ReadmeScene;

/// <summary>One step of a key script, taken after its delay.</summary>
public abstract record KeyStep(TimeSpan Delay)
{
    /// <summary>Types text a character at a time, <see cref="KeyScript.TypingInterval"/> apart.</summary>
    public sealed record Type(TimeSpan Delay, string Text) : KeyStep(Delay);

    /// <summary>Presses one named key.</summary>
    public sealed record Press(TimeSpan Delay, NamedKey Key) : KeyStep(Delay);

    /// <summary>Brings Vantage's own tab to the front again after a session tab has opened.</summary>
    public sealed record FocusVantage(TimeSpan Delay) : KeyStep(Delay);

    /// <summary>Writes a marker file the screenshot script waits for.</summary>
    public sealed record Mark(TimeSpan Delay, string Name) : KeyStep(Delay);
}