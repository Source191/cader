using Source191.Cader.Application;

namespace Source191.Cader.Core.Tests.Application;

public sealed class SharedSnapshotShould
{
    [Fact]
    public void AnswerTheFirstValue()
    {
        SharedSnapshot<string> snapshot = new("first");

        snapshot.Value.ShouldBe("first");
    }

    [Fact]
    public void AnswerTheReplacement()
    {
        SharedSnapshot<string> snapshot = new("first");

        snapshot.Replace("second");

        snapshot.Value.ShouldBe("second");
    }
}