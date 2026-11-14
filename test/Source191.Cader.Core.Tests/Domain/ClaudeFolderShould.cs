using Source191.Cader.Domain;

namespace Source191.Cader.Core.Tests.Domain;

public sealed class ClaudeFolderShould
{
    private static readonly FolderPath Home = FolderPath.From(@"C:\Users\me");

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UseTheDefaultFolderWhenTheVariableIsNotSet(string? variable)
    {
        ClaudeFolder folder = ClaudeFolder.From(variable, Home);

        folder.ShouldBe(new ClaudeFolder.Unset(FolderPath.From(@"C:\Users\me\.claude")));
    }

    [Theory]
    [InlineData(@"C:\Users\me\.claude-personal")]
    [InlineData(@"C:\Users\me\.claude-personal\")]
    public void FollowAFullPathInTheVariable(string variable)
    {
        ClaudeFolder folder = ClaudeFolder.From(variable, Home);

        folder.ShouldBe(new ClaudeFolder.FromVariable(FolderPath.From(@"C:\Users\me\.claude-personal")));
    }

    [Theory]
    [InlineData("claude-personal")]
    [InlineData(@".\claude-personal")]
    [InlineData(@"~\.claude-personal")]
    public void NotGuessAtAValueThatIsNotAFullPath(string variable)
    {
        ClaudeFolder folder = ClaudeFolder.From(variable, Home);

        folder.ShouldBe(new ClaudeFolder.NotUsable(FolderPath.From(@"C:\Users\me\.claude"), variable));
    }

    [Fact]
    public void NotUseAValueWithANulCharacter()
    {
        ClaudeFolder folder = ClaudeFolder.From("C:\\x\0y", Home);

        folder.ShouldBeOfType<ClaudeFolder.NotUsable>();
    }
}