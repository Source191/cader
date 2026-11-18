using System.Diagnostics.CodeAnalysis;

namespace Source191.Cader.Domain;

/// <summary>
/// A Claude organisation's id, a GUID as <c>claude auth status</c> names it (<c>orgId</c>) and a transcript's
/// <c>session-owner</c> line records it (<c>organizationUuid</c>). Held in lower case, so the two spellings compare equal.
/// </summary>
public sealed record OrganisationId : ITextValue<OrganisationId>
{
    private OrganisationId(string text)
    {
        if (!IsGuid(text))
        {
            throw new ArgumentException($"{text} is not an organisation id: a GUID.", nameof(text));
        }

        Value = text.ToLowerInvariant();
    }

    public static OrganisationId From(string text) => new(text);

    public string Value { get; }

    public static bool TryParse(string? text, [MaybeNullWhen(false)] out OrganisationId value)
    {
        value = text is not null && IsGuid(text) ? OrganisationId.From(text) : null;
        return value is not null;
    }

    public override string ToString() => Value;

    private static bool IsGuid(string text) => Guid.TryParseExact(text, "D", out _);
}