namespace Source191.Cader.Domain;

/// <summary>Whether a Windows Terminal tab's title shows a session's name.</summary>
public static class TabTitle
{
    public static bool Shows(string tabName, string sessionTitle) => string.Equals(tabName, sessionTitle, StringComparison.Ordinal) || tabName.EndsWith(" " + sessionTitle, StringComparison.Ordinal);
}