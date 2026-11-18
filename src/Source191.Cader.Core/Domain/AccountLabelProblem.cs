namespace Source191.Cader.Domain;

/// <summary>Why a typed account label is refused; <c>Wording</c> says it. <see cref="Taken"/> depends on the other accounts, so only the Config tab's reducer answers it.</summary>
public enum AccountLabelProblem
{
    Blank,
    TooLong,
    ControlCharacter,
    Taken,
}
