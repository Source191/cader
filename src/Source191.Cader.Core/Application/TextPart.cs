namespace Source191.Cader.Application;

/// <summary>A piece of the frame's text in its tone, such as "◐ 2 need input" in amber or "Cader " in orange.</summary>
public sealed record TextPart(string Text, TextTone Tone);