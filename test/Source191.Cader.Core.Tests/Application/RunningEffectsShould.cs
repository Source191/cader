using Source191.Cader.Application;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using Source191.Cader.TestDoubles;
using static Source191.Cader.Core.Tests.Application.Actions.SessionTestData;

namespace Source191.Cader.Core.Tests.Application;

public sealed class RunningEffectsShould
{
    private readonly RunningEffects running = new();

    [Fact]
    public void RefuseAnEqualEffectWhileItRuns()
    {
        bool first = running.TryBegin(new StopSession(Job(Worktree)));
        bool second = running.TryBegin(new StopSession(Job(Worktree)));

        first.ShouldBeTrue();
        second.ShouldBeFalse();
    }

    [Fact]
    public void AllowAnotherRowsEffect()
    {
        Session.Background other = new(SessionId.From("sess-2"), JobId.From("4f2a9c11"), Listed.As("fix-456"), FolderPath.From(@"C:\repos\Northwind.Orders-fix-456"), Fixtures.Noon, DisplayState.Working, new JobProcess.None());

        running.TryBegin(new StopSession(Job(Worktree)));

        running.TryBegin(new StopSession(other)).ShouldBeTrue();
    }

    [Fact]
    public void AllowItAgainOnceEnded()
    {
        running.TryBegin(new StopSession(Job(Worktree)));
        running.End(new StopSession(Job(Worktree)));

        running.TryBegin(new StopSession(Job(Worktree))).ShouldBeTrue();
    }

    [Fact]
    public void TellApartEffectsOfOneRow()
    {
        running.TryBegin(new StopSession(Job(Worktree)));

        running.TryBegin(new DeleteSession(Job(Worktree))).ShouldBeTrue();
    }
}