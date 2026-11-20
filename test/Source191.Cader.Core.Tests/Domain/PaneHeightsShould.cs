using Source191.Cader.Domain;

namespace Source191.Cader.Core.Tests.Domain;

public sealed class PaneHeightsShould
{
    [Fact]
    public void BeSixAndTenWhenUnset()
    {
        PaneHeights.WhenUnset.ShouldBe(new PaneHeights(Sessions: 6, Worktrees: 10));
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(4, 20)]
    [InlineData(6, 200)]
    public void KeepHeightsInRange(int sessions, int worktrees)
    {
        PaneHeights.FromFile(sessions, worktrees).ShouldBe(new PaneHeights(sessions, worktrees));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(0, 0)]
    [InlineData(7, 201)]
    [InlineData(-1, -5)]
    public void ReadAMissingOrOutOfRangeHeightAsItsDefault(int? sessions, int? worktrees)
    {
        PaneHeights.FromFile(sessions, worktrees).ShouldBe(PaneHeights.WhenUnset);
    }

    [Fact]
    public void ReadEachHeightOnItsOwn()
    {
        PaneHeights.FromFile(9, 20).ShouldBe(new PaneHeights(Sessions: 6, Worktrees: 20));
        PaneHeights.FromFile(4, 0).ShouldBe(new PaneHeights(Sessions: 4, Worktrees: 10));
    }
}