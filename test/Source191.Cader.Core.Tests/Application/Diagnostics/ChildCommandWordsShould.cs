using Source191.Cader.Application.Diagnostics;
using Source191.Cader.Ports;

namespace Source191.Cader.Core.Tests.Application.Diagnostics;

public sealed class ChildCommandWordsShould
{
    private const string AnArgumentThatIsNoWord = @"C:\Users\me\source\repos\Northwind.Orders";

    private static readonly (string Program, string[] Words)[] Listed =
    [
        ("git", ["worktree", "list", "add", "remove", "prune", "status", "rev-list", "log", "branch", "symbolic-ref", "merge-base", "rev-parse", "remote", "get-url", "ls-files", "fetch", "merge", "-c", "core.quotePath=false"]),
        ("claude", ["agents", "attach", "stop", "rm", "--version", "--bg", "--resume", "--json", "--all"]),
        ("gh", ["pr", "list", "auth", "status"]),
        ("az", ["repos", "pr", "list", "extension", "account", "show"]),
        ("wt", ["-w", "0", "new-tab"]),
        ("dotnet", ["tool", "update", "-g"]),
    ];

    public static TheoryData<string, string> EveryListedWord()
    {
        TheoryData<string, string> data = [];
        foreach ((string program, string[] words) in Listed)
        {
            foreach (string word in words)
            {
                data.Add(program, word);
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(EveryListedWord))]
    public void KeepEachListedWordOfItsProgram(string program, string word)
    {
        ProcessRequest request = new(program, [word, AnArgumentThatIsNoWord], WorkingFolder.Inherit);

        ChildCommand.For(request).ToString().ShouldBe($"{program} {word}");
    }

    [Theory]
    [InlineData("git")]
    [InlineData("claude")]
    [InlineData("gh")]
    [InlineData("az")]
    [InlineData("wt")]
    [InlineData("dotnet")]
    [InlineData("pwsh")]
    [InlineData("vswhere")]
    public void KeepNoArgumentThatIsNoListedWord(string program)
    {
        ProcessRequest request = new(program, [AnArgumentThatIsNoWord, "worktree"], WorkingFolder.Inherit);

        ChildCommand.For(request).ToString().ShouldBe(program);
    }

    [Theory]
    [InlineData("pwsh", "-NoProfile")]
    [InlineData("vswhere", "-latest")]
    [InlineData("MyTool", "list")]
    public void ListNoWordForAProgramWithNone(string program, string argument)
    {
        ChildCommandWords.None.ShouldBeEmpty();
        ChildCommand.For(new ProcessRequest(program, [argument], WorkingFolder.Inherit)).ToString().ShouldNotContain(argument);
    }

    [Fact]
    public void KeepAProgramsWordsApartFromTheOthers()
    {
        ProcessRequest request = new("git", ["agents"], WorkingFolder.Inherit);

        ChildCommand.For(request).ToString().ShouldBe("git");
    }
}