using System.Diagnostics.CodeAnalysis;

namespace Source191.Cader.Domain;

/// <summary>
/// The short name the user gives a Claude account, shown in the Sessions tab's ACCOUNT column: 1 to 8 characters with
/// no surrounding spaces and no control characters. A new account's label is a number (<see cref="AccountLabels.NextFree"/>).
/// </summary>
public sealed record AccountLabel : ITextValue<AccountLabel>
{
    public const int MostCharacters = 8;

    private AccountLabel(string text)
    {
        if (text != text.Trim() || HasProblem(text, out _))
        {
            throw new ArgumentException($"Not an account label: {text}.", nameof(text));
        }

        Value = text;
    }

    public static AccountLabel From(string text) => new(text);

    public string Value { get; }

    /// <summary>Checks typed text once trimmed: refused when blank, longer than <see cref="MostCharacters"/> or holding a control character.</summary>
    public static AccountLabelValidation Validate(string text)
    {
        string trimmed = text.Trim();

        return HasProblem(trimmed, out AccountLabelProblem problem)
            ? new AccountLabelValidation.Refused(problem)
            : new AccountLabelValidation.Valid(AccountLabel.From(trimmed));
    }

    public static bool TryParse(string? text, [MaybeNullWhen(false)] out AccountLabel value)
    {
        value = text is not null && text == text.Trim() && !HasProblem(text, out _) ? AccountLabel.From(text) : null;
        return value is not null;
    }

    public override string ToString() => Value;

    private static bool HasProblem(string text, out AccountLabelProblem problem)
    {
        (bool refused, problem) = text switch
        {
            { Length: 0 } => (true, AccountLabelProblem.Blank),
            { Length: > MostCharacters } => (true, AccountLabelProblem.TooLong),
            _ when text.Any(char.IsControl) => (true, AccountLabelProblem.ControlCharacter),
            _ => (false, default(AccountLabelProblem)),
        };

        return refused;
    }
}
