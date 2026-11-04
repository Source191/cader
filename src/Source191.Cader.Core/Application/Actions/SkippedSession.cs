using Source191.Cader.Domain;

namespace Source191.Cader.Application.Actions;

/// <summary>A session close-empty left alone because it has messages by the time it runs.</summary>
public sealed record SkippedSession(ListedName Name);