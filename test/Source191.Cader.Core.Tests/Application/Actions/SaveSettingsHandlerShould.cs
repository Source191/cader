using Source191.Cader.Application;
using Source191.Cader.Application.Actions;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application.Actions;

public sealed class SaveSettingsHandlerShould
{
    [Fact]
    public async Task SaveThroughCurrentSettingsAndReportSettingsSaved()
    {
        InMemoryState state = new();
        CurrentSettings current = new(TestSettings.For(@"C:\Users\me\source\repos"), state);
        Settings saved = TestSettings.For([@"C:\Users\me\source\repos", @"D:\work"], @"D:\worktrees");

        object outcome = await new SaveSettingsHandler(current).HandleAsync(new SaveSettings(saved), CancellationToken.None);

        outcome.ShouldBe(new SaveSettingsOutcome());
        current.Value.ShouldBe(saved);
        SettingsFile.From(await state.ReadConfigAsync(), TestSettings.Home).ShouldBe(saved);
    }
}