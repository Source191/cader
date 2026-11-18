using Source191.Cader.Application.Launch;

namespace Source191.Cader.Application.Views;

/// <summary>One row of the wizard's list: a group's heading, which the selection never rests on, or a choice.</summary>
public abstract record WizardRow
{
    /// <summary>Each group that has a choice, as its heading and then its choices, in the order the groups are listed.</summary>
    public static IReadOnlyList<WizardRow> For(RepositoryChoices choices) =>
    [
        .. Listed(ChoiceGroup.Recent, choices.Recent),
        .. Listed(ChoiceGroup.Folders, choices.Folders),
        .. Listed(ChoiceGroup.Repositories, choices.Repositories),
    ];

    private static IEnumerable<WizardRow> Listed(ChoiceGroup group, IReadOnlyList<RepositoryChoice> choices) =>
        choices.Count == 0 ? [] : [new Heading(group), .. choices.Select(choice => new Offered(choice))];

    public sealed record Heading(ChoiceGroup Group) : WizardRow;

    public sealed record Offered(RepositoryChoice Choice) : WizardRow;
}