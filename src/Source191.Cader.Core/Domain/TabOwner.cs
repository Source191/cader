namespace Source191.Cader.Domain;

/// <summary>Whose tab a registration is: a session's, or Cader's own.</summary>
public abstract record TabOwner
{
    public static readonly TabOwner Console = new ForConsole();

    public sealed record ForSession(SessionId Id) : TabOwner;

    /// <summary>The tab Cader itself runs in, so its colour is not handed to a session tab.</summary>
    public sealed record ForConsole : TabOwner;
}