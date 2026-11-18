using Source191.Cader.Domain;

namespace Source191.Cader.Ports;

/// <summary>
/// Who <c>claude auth status --json</c> says is logged in: an organisation with its email and name; LoggedOut when it
/// says no one is, or names no email or organisation (an API key gives none); NotRead when it cannot be run or read.
/// </summary>
public abstract record ClaudeLogin
{
    public sealed record LoggedIn(OrganisationId Organisation, string Email, string OrganisationName) : ClaudeLogin;

    public sealed record LoggedOut : ClaudeLogin;

    public sealed record NotRead : ClaudeLogin;
}
