using Source191.Cader.Application.Views;

namespace Source191.Cader.Core.Tests.Application.Views;

public sealed class SettingsScopeShould
{
    [Fact]
    public void NameTheRowThatHasNoDialog()
    {
        ArgumentException refusal = Should.Throw<ArgumentException>(() => SettingsScope.For(new ConfigRow.CheckForUpdates(On: true)));

        refusal.Message.ShouldStartWith("A CheckForUpdates row has no dialog.");
        refusal.ParamName.ShouldBe("row");
    }
}