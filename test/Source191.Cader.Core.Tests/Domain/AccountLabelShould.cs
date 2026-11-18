using Source191.Cader.Domain;

namespace Source191.Cader.Core.Tests.Domain;

public sealed class AccountLabelShould
{
    [Fact]
    public void AcceptATypedLabel()
    {
        AccountLabel.Validate("work").ShouldBe(new AccountLabelValidation.Valid(AccountLabel.From("work")));
    }

    [Fact]
    public void TrimTheTypedLabel()
    {
        AccountLabel.Validate("  work ").ShouldBe(new AccountLabelValidation.Valid(AccountLabel.From("work")));
    }

    [Fact]
    public void AcceptEightCharacters()
    {
        AccountLabel.Validate("personal").ShouldBeOfType<AccountLabelValidation.Valid>();
    }

    [Fact]
    public void RefuseNineCharacters()
    {
        AccountLabel.Validate("northwind").ShouldBe(new AccountLabelValidation.Refused(AccountLabelProblem.TooLong));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void RefuseABlankLabel(string text)
    {
        AccountLabel.Validate(text).ShouldBe(new AccountLabelValidation.Refused(AccountLabelProblem.Blank));
    }

    [Fact]
    public void RefuseAControlCharacter()
    {
        AccountLabel.Validate("wo\u001brk").ShouldBe(new AccountLabelValidation.Refused(AccountLabelProblem.ControlCharacter));
    }

    [Theory]
    [InlineData(" work")]
    [InlineData("northwind")]
    [InlineData("")]
    public void ThrowFromForTextThatBreaksTheRules(string text)
    {
        Should.Throw<ArgumentException>(() => AccountLabel.From(text));
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" 1", false)]
    [InlineData("1", true)]
    public void ParseOnlyAGoodLabel(string? text, bool parses)
    {
        AccountLabel.TryParse(text, out AccountLabel? label).ShouldBe(parses);
        (label is not null).ShouldBe(parses);
    }
}
