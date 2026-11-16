using Source191.Cader.Domain;

namespace Source191.Cader.Core.Tests.Domain;

public sealed class TabTitleShould
{
    [Fact]
    public void ShowASessionWhoseNameIsTheWholeTitle()
    {
        TabTitle.Shows("fix-123", "fix-123").ShouldBeTrue();
    }
}
