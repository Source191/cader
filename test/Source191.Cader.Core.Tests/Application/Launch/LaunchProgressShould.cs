using Source191.Cader.Application.Launch;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;

namespace Source191.Cader.Core.Tests.Application.Launch;

public sealed class LaunchProgressShould
{
    private static readonly RepositoryChoice Repository = new("Northwind.Orders", FolderPath.From(@"C:\repos\Northwind.Orders"), IsRepository: true);

    private readonly LaunchProgress progress = new();

    [Fact]
    public void StartWithNoStep()
    {
        progress.Latest.ShouldBe(LaunchStep.None);
        StatusMessage working = new StatusMessage.Working(new StartSession(Plan("none")));

        progress.Shown(working).ShouldBeSameAs(working, "no step has begun, so even a launch named like the placeholder shows its plain words");
    }

    [Fact]
    public void KeepTheLatestStep()
    {
        LaunchSteps steps = LaunchSteps.WithoutANewWorktree(SessionName.From("fix-123"));

        progress.Report(steps.Step(LaunchStage.StartingClaude));
        progress.Report(steps.Step(LaunchStage.FindingSession));

        progress.Latest.ShouldBe(steps.Step(LaunchStage.FindingSession));
    }

    [Fact]
    public void ReportWithoutCallingAnyone()
    {
        LaunchStep step = LaunchSteps.WithoutANewWorktree(SessionName.From("fix-123")).Step(LaunchStage.StartingClaude);

        progress.Report(step);

        progress.Latest.ShouldBe(step);
        typeof(LaunchProgress).GetMethods().Where(method => method.DeclaringType == typeof(LaunchProgress)).Select(method => method.Name).Order(StringComparer.Ordinal).ToList()
            .ShouldBe(["Report", "Shown", "get_Latest"], customMessage: "a launch's thread only replaces the step, and the screen's ticker reads it, so the launch never waits on the UI thread");
    }

    [Fact]
    public void CallItsListenerOnEachStep()
    {
        LaunchSteps steps = LaunchSteps.WithoutANewWorktree(SessionName.From("fix-123"));
        List<LaunchStep> heard = [];
        progress.Report(steps.Step(LaunchStage.StartingClaude));
        progress.Follow(() => heard.Add(progress.Latest));

        progress.Report(steps.Step(LaunchStage.FindingSession));
        progress.Report(steps.Step(LaunchStage.OpeningTab));

        heard.ShouldBe([steps.Step(LaunchStage.FindingSession), steps.Step(LaunchStage.OpeningTab)]);
    }

    [Fact]
    public void ShowTheStepForTheLaunchItNames()
    {
        LaunchStep step = LaunchSteps.WithoutANewWorktree(SessionName.From("fix-123")).Step(LaunchStage.StartingClaude);
        progress.Report(step);

        progress.Shown(new StatusMessage.Working(new StartSession(Plan("fix-123")))).ShouldBe(new StatusMessage.Launching(step));
    }

    [Fact]
    public void LeaveAnotherLaunchsMessageAlone()
    {
        progress.Report(LaunchSteps.WithoutANewWorktree(SessionName.From("fix-123")).Step(LaunchStage.StartingClaude));
        StatusMessage another = new StatusMessage.Working(new StartSession(Plan("fix-456")));
        StatusMessage stopping = new StatusMessage.Working(new Quit());
        StatusMessage blank = StatusMessage.None;

        progress.Shown(another).ShouldBeSameAs(another);
        progress.Shown(stopping).ShouldBeSameAs(stopping);
        progress.Shown(blank).ShouldBeSameAs(blank);
    }

    private static LaunchPlan Plan(string name) => new(
        Repository.Path, SessionName.From(name), new LaunchPlace.CurrentBranch(),
        WorktreeBase.RemoteDefault, new OpeningPrompt.None(), Repository, BeforeBranching.Fetch);
}