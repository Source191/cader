using Source191.Cader.Domain;

namespace Source191.Cader.Core.Tests.Domain;

public sealed class OpeningPromptShould
{
    [Fact]
    public void HoldTheTextGiven()
    {
        OpeningPrompt prompt = new OpeningPrompt.Text("Fix the failing tests");

        prompt.ShouldBeOfType<OpeningPrompt.Text>().Words.ShouldBe("Fix the failing tests");
        prompt.ShouldNotBe(new OpeningPrompt.None());
        new OpeningPrompt.None().ShouldBe(new OpeningPrompt.None());
    }
}