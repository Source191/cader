using Source191.Cader.Domain;
using Source191.Cader.Ports;

namespace Source191.Cader.Core.Tests.PortTypes;

public sealed class UserConfigShould
{
    public static TheoryData<UserConfig> ConfigsThatDifferFromTheBase =>
    [
        Variant(roots: [@"D:\work"]),
        Variant(defaultBase: WorktreeBase.Head),
        Variant(worktreeRoot: @"D:\trees"),
        Variant(editor: new EditorSetting("Rider")),
        Variant(checkForUpdates: false),
        Variant(defaultPlace: LaunchMode.CurrentBranch),
        Variant(diagnosticLog: true),
        Variant(beforeBranching: BeforeBranching.Pull),
    ];

    [Theory]
    [MemberData(nameof(ConfigsThatDifferFromTheBase))]
    public void BeUnequalWhenAnyMemberDiffers(UserConfig different)
    {
        Variant().Equals(different).ShouldBeFalse();
        different.Equals(Variant()).ShouldBeFalse();
    }

    [Fact]
    public void BeEqualWhenEveryMemberMatches()
    {
        Variant().Equals(Variant()).ShouldBeTrue();
        Variant().Equals(null).ShouldBeFalse();
    }

    [Fact]
    public void ReadMissingRootsAsNoRootsFromEitherSide()
    {
        new UserConfig(RepositoryRoots: null).Equals(new UserConfig(RepositoryRoots: [])).ShouldBeTrue();
        new UserConfig(RepositoryRoots: []).Equals(new UserConfig(RepositoryRoots: null)).ShouldBeTrue();
        UserConfig.Missing.Equals(new UserConfig()).ShouldBeTrue();
    }

    private static UserConfig Variant(
        IReadOnlyList<string>? roots = null,
        WorktreeBase defaultBase = WorktreeBase.RemoteDefault,
        string worktreeRoot = @"C:\Users\me\source\worktrees",
        EditorSetting? editor = null,
        bool checkForUpdates = true,
        LaunchMode defaultPlace = LaunchMode.NewWorktree,
        bool diagnosticLog = false,
        BeforeBranching beforeBranching = BeforeBranching.Fetch)
    {
        return new UserConfig(roots ?? [@"C:\Users\me\source\repos"], defaultBase, worktreeRoot, editor ?? new EditorSetting("VsCode"), checkForUpdates, defaultPlace, diagnosticLog, beforeBranching);
    }
}