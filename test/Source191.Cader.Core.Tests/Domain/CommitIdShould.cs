using Source191.Cader.Domain;

namespace Source191.Cader.Core.Tests.Domain;

public sealed class CommitIdShould
{
    [Theory]
    [InlineData("afdacd8b1c2e3f4a5b6c7d8e9f0a1b2c3d4e5f60")]
    [InlineData("AFDACD8B1C2E3F4A5B6C7D8E9F0A1B2C3D4E5F60")]
    [InlineData("afdacd8b1c2e3f4a5b6c7d8e9f0a1b2c3d4e5f60afdacd8b1c2e3f4a5b6c7d8e")]
    public void ReadFortyOrSixtyFourHexDigits(string text)
    {
        CommitId.From(text).Value.ShouldBe(text);
        CommitId.TryParse(text, out CommitId? parsed).ShouldBeTrue();
        parsed.ShouldBe(CommitId.From(text));
        $"{parsed}".ShouldBe(text);
    }

    [Theory]
    [InlineData("afdacd8b1c2e3f4a5b6c7d8e9f0a1b2c3d4e5f6")]
    [InlineData("afdacd8b1c2e3f4a5b6c7d8e9f0a1b2c3d4e5f600")]
    [InlineData("gfdacd8b1c2e3f4a5b6c7d8e9f0a1b2c3d4e5f60")]
    [InlineData("afdacd8b")]
    [InlineData("")]
    [InlineData("   ")]
    public void RefuseAnythingElse(string text)
    {
        Should.Throw<ArgumentException>(() => CommitId.From(text));
        CommitId.TryParse(text, out CommitId? parsed).ShouldBeFalse();
        parsed.ShouldBeNull();
    }

    [Fact]
    public void SayWhatItRefusedAndWhatItNeeds()
    {
        ArgumentException refused = Should.Throw<ArgumentException>(() => CommitId.From("afdacd8b"));

        refused.Message.ShouldStartWith("'afdacd8b' is not a commit id: it needs 40 or 64 hex digits.");
    }

    [Fact]
    public void AnswerFalseForNull()
    {
        CommitId.TryParse(null, out CommitId? parsed).ShouldBeFalse();
        parsed.ShouldBeNull();
    }
}