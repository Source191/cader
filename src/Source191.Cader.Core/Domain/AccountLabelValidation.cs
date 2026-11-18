namespace Source191.Cader.Domain;

/// <summary>A typed label checked against the rules: the label, or the reason it is refused.</summary>
public abstract record AccountLabelValidation
{
    public sealed record Valid(AccountLabel Label) : AccountLabelValidation;

    public sealed record Refused(AccountLabelProblem Problem) : AccountLabelValidation;
}
