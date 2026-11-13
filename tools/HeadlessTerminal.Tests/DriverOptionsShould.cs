namespace Source191.Cader.HeadlessTerminal.Tests;

public sealed class DriverOptionsShould
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TurnTheUpdateCheckOffForTheChild(bool pretendWindowsTerminal)
    {
        DriverOptions options = new(pretendWindowsTerminal, AnswerQueries: false, XtversionDelayMs: 0);

        options.ChildEnvironment()["CADER_NO_UPDATE_CHECK"].ShouldBe("1");
    }
}