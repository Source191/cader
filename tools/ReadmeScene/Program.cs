namespace Source191.Vantage.ReadmeScene;

/// <summary>
/// Makes the README's screenshot and animation: Vantage's real console screen over a made-up scene of Northwind.*
/// repositories, with scripted Claude Code, gh and az answers, run by <c>tools\New-ReadmeScreenshot.ps1</c>.
/// </summary>
public static class Program
{
    public static int Main(string[] args) => args switch
    {
        _ => Usage(),
    };

    private static int Usage()
    {
        Console.Error.WriteLine("Usage: readme-scene run <scene-root> | attach <name> | claim-tab <id> <pid> | capture <title> <file>");
        return 2;
    }
}
