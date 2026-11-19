using Source191.Cader.Domain;

namespace Source191.Cader.Core.Tests.Domain;

public sealed class OrganisationIdShould
{
    private const string Northwind = "6f1c2a8e-4b7d-4c1e-9a3f-2d5e8b0c7a91";

    [Fact]
    public void ParseAGuid()
    {
        OrganisationId.TryParse(Northwind, out OrganisationId? id).ShouldBeTrue();

        id.ShouldBe(OrganisationId.From(Northwind));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("northwind")]
    [InlineData("6f1c2a8e4b7d4c1e9a3f2d5e8b0c7a91")]
    [InlineData("{6f1c2a8e-4b7d-4c1e-9a3f-2d5e8b0c7a91}")]
    public void RefuseTextThatIsNotAGuid(string? text)
    {
        OrganisationId.TryParse(text, out OrganisationId? id).ShouldBeFalse();
        id.ShouldBeNull();
    }

    [Fact]
    public void ThrowFromForTextThatIsNotAGuid()
    {
        Should.Throw<ArgumentException>(() => OrganisationId.From("northwind")).Message.ShouldStartWith("northwind is not an organisation id: a GUID.");
    }

    [Fact]
    public void EqualTheSameGuidInCapitals()
    {
        OrganisationId.From(Northwind.ToUpperInvariant()).ShouldBe(OrganisationId.From(Northwind));
    }
}