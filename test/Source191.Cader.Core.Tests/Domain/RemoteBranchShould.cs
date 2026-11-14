using Source191.Cader.Domain;

namespace Source191.Cader.Core.Tests.Domain;

public sealed class RemoteBranchShould
{
    [Fact]
    public void SplitTheRemoteFromTheBranch()
    {
        RemoteBranch.TryParse("origin/main", out RemoteBranch? parsed).ShouldBeTrue();

        parsed.ShouldNotBeNull().Remote.ShouldBe("origin");
        parsed.Branch.ShouldBe(BranchName.From("main"));
        parsed.Value.ShouldBe("origin/main");
        $"{parsed}".ShouldBe("origin/main");
    }

    [Fact]
    public void KeepEveryLaterSlashInTheBranch()
    {
        RemoteBranch branch = RemoteBranch.From("origin/release/2026");

        branch.Remote.ShouldBe("origin");
        branch.Branch.ShouldBe(BranchName.From("release/2026"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("main")]
    [InlineData("origin/")]
    [InlineData("/main")]
    [InlineData("origin/fix 123")]
    [InlineData("my origin/main")]
    [InlineData("-x/main")]
    public void AnswerFalseForTextThatNamesNoRemoteBranch(string? text)
    {
        RemoteBranch.TryParse(text, out RemoteBranch? branch).ShouldBeFalse();
        branch.ShouldBeNull();
    }

    [Theory]
    [InlineData("main")]
    [InlineData("origin/")]
    public void ThrowFromTextThatDoesNotParse(string text)
    {
        Should.Throw<ArgumentException>(() => RemoteBranch.From(text));
    }
}
