using Source191.Cader.Domain;

namespace Source191.Cader.Core.Tests.Domain;

public sealed class InheritedEnvironmentShould
{
    [Theory]
    [InlineData("NO_COLOR")]
    [InlineData("CLAUDECODE")]
    [InlineData("CLAUDE_PID")]
    [InlineData("CLAUDE_CODE_ENTRYPOINT")]
    [InlineData("CLAUDE_CODE_SESSION_ID")]
    [InlineData("claude_code_child_session")]
    [InlineData("GIT_TERMINAL_PROMPT")]
    public void DropWhatAClaudeCodeSessionLeavesInTheEnvironmentSoChildrenRunAsIfStartedByHand(string name)
    {
        InheritedEnvironment.ShouldDrop(name).ShouldBeTrue();
    }

    [Theory]
    [InlineData("PATH")]
    [InlineData("WT_SESSION")]
    [InlineData("USERPROFILE")]
    [InlineData("CLAUDE_CONFIG_DIR")]
    public void KeepEverythingElseIncludingTheConfigDirectoryThatSelectsAnAccount(string name)
    {
        InheritedEnvironment.ShouldDrop(name).ShouldBeFalse();
    }
}