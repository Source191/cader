namespace Source191.Cader.ReadmeScene;

/// <summary>A made-up Claude account: its label, an email at northwind.example, its organisation and id, and whether it is the scene's login.</summary>
public sealed record SceneAccount(string Label, string Email, string Organisation, string OrganisationId, bool LoggedIn = false);