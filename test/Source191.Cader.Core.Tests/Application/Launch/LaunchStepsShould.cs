using Source191.Cader.Application.Launch;
using Source191.Cader.Domain;

namespace Source191.Cader.Core.Tests.Application.Launch;

public sealed class LaunchStepsShould
{
    private static readonly SessionName Name = SessionName.From("fix-123");
    private static readonly RemoteBranch OriginMain = RemoteBranch.From("origin/main");

    [Fact]
    public void CountFiveStepsForAFetchedNewWorktree()
    {
        LaunchSteps fetched = LaunchSteps.ForNewWorktree(Name, OriginMain, BeforeBranching.Fetch);
        LaunchSteps pulled = LaunchSteps.ForNewWorktree(Name, OriginMain, BeforeBranching.Pull);

        fetched.Step(LaunchStage.Fetching).ShouldBe(new LaunchStep(Name, LaunchStage.Fetching, 1, 5, new LaunchBase.Remote(OriginMain)));
        pulled.Step(LaunchStage.Pulling).ShouldBe(new LaunchStep(Name, LaunchStage.Pulling, 1, 5, new LaunchBase.Remote(OriginMain)));
        pulled.Step(LaunchStage.OpeningTab).Number.ShouldBe(5);
    }

    [Fact]
    public void CountFourWithoutAnUpdate()
    {
        LaunchSteps nothing = LaunchSteps.ForNewWorktree(Name, OriginMain, BeforeBranching.None);
        LaunchSteps fromHead = LaunchSteps.ForNewWorktree(Name);

        nothing.Step(LaunchStage.CreatingWorktree).ShouldBe(new LaunchStep(Name, LaunchStage.CreatingWorktree, 1, 4, new LaunchBase.Remote(OriginMain)));
        fromHead.Step(LaunchStage.CreatingWorktree).ShouldBe(new LaunchStep(Name, LaunchStage.CreatingWorktree, 1, 4, new LaunchBase.None()));
        fromHead.Step(LaunchStage.OpeningTab).Number.ShouldBe(4);
    }

    [Fact]
    public void CountThreeWithoutANewWorktree()
    {
        LaunchSteps existing = LaunchSteps.WithoutANewWorktree(Name);

        existing.Step(LaunchStage.StartingClaude).ShouldBe(new LaunchStep(Name, LaunchStage.StartingClaude, 1, 3, new LaunchBase.None()));
        existing.Step(LaunchStage.OpeningTab).ShouldBe(new LaunchStep(Name, LaunchStage.OpeningTab, 3, 3, new LaunchBase.None()));
    }

    [Fact]
    public void NumberEachStageInOrder()
    {
        LaunchSteps steps = LaunchSteps.ForNewWorktree(Name, OriginMain, BeforeBranching.Fetch);

        LaunchStage[] stages = [LaunchStage.Fetching, LaunchStage.CreatingWorktree, LaunchStage.StartingClaude, LaunchStage.FindingSession, LaunchStage.OpeningTab];
        stages.Select(stage => steps.Step(stage).Number).ShouldBe([1, 2, 3, 4, 5]);
        Should.Throw<ArgumentException>(() => steps.Step(LaunchStage.Pulling));
    }
}
