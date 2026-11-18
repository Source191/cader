using Source191.Cader.Domain;

namespace Source191.Cader.Core.Tests.Domain;

public sealed class AccountLabelsShould
{
    [Fact]
    public void NumberTheFirstAccountOne()
    {
        AccountLabels.NextFree([]).ShouldBe(AccountLabel.From("1"));
    }

    [Fact]
    public void ReuseOneOnceTheFirstAccountIsRenamed()
    {
        AccountLabels.NextFree([AccountLabel.From("work")]).ShouldBe(AccountLabel.From("1"));
    }

    [Fact]
    public void FillAGap()
    {
        AccountLabels.NextFree([AccountLabel.From("1"), AccountLabel.From("3")]).ShouldBe(AccountLabel.From("2"));
    }

    [Fact]
    public void FindALabelTakenInAnotherCase()
    {
        AccountLabels.IsTaken([AccountLabel.From("work")], AccountLabel.From("Work")).ShouldBeTrue();
    }

    [Fact]
    public void FindALabelFree()
    {
        AccountLabels.IsTaken([AccountLabel.From("work")], AccountLabel.From("home")).ShouldBeFalse();
    }
}