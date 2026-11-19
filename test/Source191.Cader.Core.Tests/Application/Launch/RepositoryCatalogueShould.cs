using Source191.Cader.Application;
using Source191.Cader.Application.Launch;
using Source191.Cader.Domain;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application.Launch;

public sealed class RepositoryCatalogueShould
{
    private const string Root = @"C:\Users\me\source\repos";

    private readonly FakeFolders folders = new FakeFolders()
        .WithDirectory(Path.Combine(Root, "Northwind.Agents", ".git"))
        .WithDirectory(Path.Combine(Root, "Northwind.Templates", ".git"))
        .WithDirectory(Path.Combine(Root, "Northwind.Billing", ".git"))
        .WithFile(Path.Combine(Root, "Northwind.Billing-fix-123", ".git"), "gitdir: C:/Users/me/source/repos/Northwind.Billing/.git/worktrees/Northwind.Billing-fix-123")
        .WithDirectory(Path.Combine(Root, "notes"));

    private RepositoryCatalogue Catalogue => new(folders, TestSettings.Current(Root));

    [Fact]
    public void ListEveryGitRepositoryDirectlyUnderTheRootsAlphabetically()
    {
        RepositoryChoices choices = Catalogue.Choices(recent: []);

        choices.Recent.ShouldBeEmpty();
        choices.Repositories.Select(choice => choice.Name).ShouldBe(["Northwind.Agents", "Northwind.Billing", "Northwind.Templates"]);
        choices.Repositories.ShouldAllBe(choice => choice.IsRepository);
    }

    [Fact]
    public void ListTheRootsAsFolderChoicesByTheirFullPaths()
    {
        const string Work = @"D:\work";
        folders.WithDirectory(Work);
        RepositoryCatalogue catalogue = new(folders, TestSettings.Current(Root, Work));

        RepositoryChoices choices = catalogue.Choices(recent: [FolderPath.From(Path.Combine(Root, "Northwind.Templates"))]);

        choices.Recent.Select(choice => choice.Name).ShouldBe(["Northwind.Templates"]);
        choices.Folders.ShouldBe([new RepositoryChoice(Root, FolderPath.From(Root), IsRepository: false), new RepositoryChoice(Work, FolderPath.From(Work), IsRepository: false)]);
        choices.Repositories.Select(choice => choice.Name).ShouldBe(["Northwind.Agents", "Northwind.Billing"]);
    }

    [Fact]
    public void PutRecentlyUsedRepositoriesFirstInRecentOrder()
    {
        IReadOnlyList<FolderPath> recent = [FolderPath.From(Path.Combine(Root, "Northwind.Agents")), FolderPath.From(Path.Combine(Root, "Northwind.Templates")), FolderPath.From(Path.Combine(Root, "gone")), FolderPath.From(Root)];

        RepositoryChoices choices = Catalogue.Choices(recent);

        choices.Recent.Select(choice => choice.Name).ShouldBe(["Northwind.Agents", "Northwind.Templates"], "a recently used root is listed with the folders, not as a recent repository");
        choices.Repositories.Select(choice => choice.Name).ShouldBe(["Northwind.Billing"]);
        choices.Folders.Select(choice => choice.Name).ShouldBe([Root]);
    }

    [Fact]
    public void FilterByNameCaseInsensitively()
    {
        RepositoryChoices choices = Catalogue.Choices(recent: []).Matching("BILL");

        choices.Repositories.Select(choice => choice.Name).ShouldBe(["Northwind.Billing"]);
        choices.Folders.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("​")]
    public void KeepEveryChoiceForAFilterOfInvisibleCharacters(string invisible)
    {
        RepositoryChoices all = Catalogue.Choices(recent: [FolderPath.From(Path.Combine(Root, "Northwind.Agents"))]);

        RepositoryChoices choices = all.Matching(invisible);

        choices.Recent.ShouldBe(all.Recent);
        choices.Folders.ShouldBe(all.Folders);
        choices.Repositories.ShouldBe(all.Repositories);
    }

    [Fact]
    public void IgnoreBlanksBeforeTheFilterButNotInsideIt()
    {
        RepositoryChoices all = Catalogue.Choices(recent: []);

        all.Matching(" bill").Repositories.ShouldBe(all.Matching("bill").Repositories);
        all.Matching(" bill").Repositories.ShouldNotBeEmpty();
        all.Matching("d b").Repositories.ShouldBeEmpty("a space inside the filter is not a dot");
    }

    [Fact]
    public void FilterAFolderChoiceByItsPath()
    {
        Catalogue.Choices(recent: []).Matching("source").Folders.Select(choice => choice.Name).ShouldBe([Root]);
    }

    [Fact]
    public void IgnoreARootThatDoesNotExist()
    {
        RepositoryCatalogue catalogue = new(folders, TestSettings.Current(Root, Path.Combine(Root, "missing")));

        RepositoryChoices choices = catalogue.Choices(recent: []);

        choices.Repositories.Count.ShouldBe(3);
        choices.Folders.Select(choice => choice.Name).ShouldBe([Root]);
    }

    [Fact]
    public void OfferMainCheckoutsOnly()
    {
        RepositoryChoices choices = Catalogue.Choices(recent: []).Matching("Billing");

        choices.Repositories.Select(choice => choice.Name).ShouldBe(["Northwind.Billing"], "a linked worktree under a root is not a repository");
    }

    [Fact]
    public void CountTheRepositoriesOffered()
    {
        Catalogue.Choices(recent: [FolderPath.From(Path.Combine(Root, "Northwind.Agents"))]).RepositoryCount.ShouldBe(3, "the folder choices are not repositories");
    }

    [Fact]
    public async Task UseTheRootsSavedAfterItWasBuilt()
    {
        const string Work = @"D:\work";
        folders.WithDirectory(Path.Combine(Work, "Northwind.Ledger", ".git"));
        CurrentSettings settings = TestSettings.Current(Root);
        RepositoryCatalogue catalogue = new(folders, settings);

        await settings.SaveAsync(TestSettings.For(Root, Work));

        catalogue.Choices(recent: []).Matching("Ledger").Repositories.Select(choice => choice.Name).ShouldBe(["Northwind.Ledger"]);
    }
}