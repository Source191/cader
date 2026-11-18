using System.Globalization;

namespace Source191.Cader.Domain;

/// <summary>The rules across every remembered account's label: a new account takes the lowest free number, and no two share a label in any case.</summary>
public static class AccountLabels
{
    public static AccountLabel NextFree(IEnumerable<AccountLabel> used)
    {
        AccountLabel[] taken = [.. used];
        int number = 1;
        while (IsTaken(taken, AccountLabel.From(number.ToString(CultureInfo.InvariantCulture))))
        {
            number++;
        }

        return AccountLabel.From(number.ToString(CultureInfo.InvariantCulture));
    }

    public static bool IsTaken(IEnumerable<AccountLabel> used, AccountLabel label) => used.Any(other => string.Equals(other.Value, label.Value, StringComparison.OrdinalIgnoreCase));
}
