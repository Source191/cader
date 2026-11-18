using Source191.Cader.Application;
using Source191.Cader.Domain;
using Source191.Cader.Ports;

namespace Source191.Cader.Core.Tests.Application;

public sealed class WordingOfAccountsShould
{
    private static readonly KnownAccount Work = new(OrganisationId.From("6f1c2a8e-4b7d-4c1e-9a3f-2d5e8b0c7a91"), "me@northwind.example", "Northwind Traders", AccountLabel.From("work"));

    [Fact]
    public void MarkTheAccountLoggedInNow()
    {
        Wording.AccountCell(new RowAccount.Known(Work, LoggedInNow: true)).ShouldBe("● work");
    }

    [Fact]
    public void ShowAnotherAccountsLabelAlone()
    {
        Wording.AccountCell(new RowAccount.Known(Work with { Label = AccountLabel.From("personal") }, LoggedInNow: false)).ShouldBe("personal");
    }

    [Fact]
    public void ShowAnOwnerNotYetSeenAsAQuestionMark()
    {
        Wording.AccountCell(new RowAccount.Unseen()).ShouldBe("?");
    }

    [Fact]
    public void LeaveTheCellBlankWhenNoOwnerIsRecorded()
    {
        Wording.AccountCell(new RowAccount.Unrecorded()).ShouldBeEmpty();
    }

    [Fact]
    public void ExplainTheColumnInTheHelp()
    {
        Wording.AccountColumn.Meaning.ShouldSatisfyAllConditions(
            meaning => meaning.ShouldContain("the account it belongs to"),
            meaning => meaning.ShouldContain("● logged in now"),
            meaning => meaning.ShouldContain("? not yet seen logged in"),
            meaning => meaning.ShouldContain("Config lists and renames them"));
    }
}
