using Source191.Cader.Application.Launch;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application.Views;

/// <summary>What the new-session wizard opens with, for a repository to be chosen and for a fixed folder.</summary>
public sealed class WizardStateShould
{
    private const string Root = @"C:\Users\me\source\repos";
    private static readonly string Billing = Path.Combine(Root, "Northwind.Billing");

    private readonly FakeFolders folders = new FakeFolders().WithDirectory(Path.Combine(Billing, ".git"));

    private RepositoryCatalogue Catalogue => new(folders, TestSettings.Current(Root));

    [Fact]
    public void OpenToChooseARepositoryWithTheFirstSelectedAndNothingTyped()
    {
        WizardState opened = WizardState.Open(Catalogue, [], folders, TestSettings.For(Root), new WizardOrigin.Anywhere());

        opened.Choices.Select(choice => choice.Name).ShouldBe([Root, "Northwind.Billing"]);
        opened.SelectedIndex.ShouldBe(1, "the first choice, after the Folders heading");
        opened.Prompt.ShouldBeEmpty();
    }

    [Fact]
    public void SelectNothingWhenThereIsNoRepositoryToChoose()
    {
        FakeFolders none = new();

        WizardState opened = WizardState.Open(new RepositoryCatalogue(none, TestSettings.Current(Root)), [], none, TestSettings.For(Root), new WizardOrigin.Anywhere());

        opened.Choices.ShouldBeEmpty();
        opened.SelectedIndex.ShouldBeNull();
    }

    [Fact]
    public void OfferNoRepositoryToChooseForAFixedFolder()
    {
        WizardState opened = WizardState.Open(Catalogue, [], folders, TestSettings.For(Root), WizardOrigin.In(FolderPath.From(Billing)));

        opened.Rows.ShouldBeEmpty();
        opened.SelectedIndex.ShouldBeNull();
        opened.Prompt.ShouldBeEmpty();
    }
}