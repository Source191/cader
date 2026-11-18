using Source191.Cader.Domain;

namespace Source191.Cader.Ports;

/// <summary>A Claude account Cader has seen logged in, by its organisation, with the email and organisation name it last saw and the label the user knows it by. The email and name are only shown, never compared or passed to a program.</summary>
public sealed record KnownAccount(OrganisationId Organisation, string Email, string OrganisationName, AccountLabel Label);