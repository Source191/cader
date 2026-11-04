namespace Source191.Cader.Application.Launch;

/// <summary>
/// The session name <c>cader here</c> was given: left off, so the folder's name is used, or typed (empty text
/// included, which is refused as a blank name).
/// </summary>
public abstract record HereName
{
    public sealed record FolderName : HereName;

    public sealed record Typed(string Text) : HereName;
}