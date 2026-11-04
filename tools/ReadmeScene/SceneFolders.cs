namespace Source191.Vantage.ReadmeScene;

/// <summary>
/// The scene's folders under its root: <c>me</c> is the home the console is given (so labels read <c>~\source\repos</c>
/// and no real user name shows), <c>state</c> stands in for <c>%LOCALAPPDATA%\Vantage</c>, <c>claude</c> for
/// <c>~\.claude</c>, and <c>bin</c> holds what the tool would carry beside itself.
/// </summary>
public sealed record SceneFolders(string Root)
{
    public string Home => Path.Combine(Root, "me");

    public string State => Path.Combine(Root, "state");

    public string ClaudeHome => Path.Combine(Root, "claude");

    public string Bin => Path.Combine(Root, "bin");
}
