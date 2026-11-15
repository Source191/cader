using Source191.Cader.Ports;

namespace Source191.Cader.Core.Tests.PortTypes;

public sealed class EditorSettingShould
{
    public static TheoryData<EditorSetting> SettingsThatDifferFromTheBase =>
    [
        new EditorSetting("Rider", @"C:\Tools\subl.exe", ["-n", "{folder}"]),
        new EditorSetting("Custom", @"C:\Tools\edit.exe", ["-n", "{folder}"]),
        new EditorSetting("Custom", @"C:\Tools\subl.exe", ["-w", "{folder}"]),
    ];

    [Theory]
    [MemberData(nameof(SettingsThatDifferFromTheBase))]
    public void BeUnequalWhenAnyMemberDiffers(EditorSetting different)
    {
        Base().Equals(different).ShouldBeFalse();
        different.Equals(Base()).ShouldBeFalse();
    }

    [Fact]
    public void BeEqualWhenEveryMemberMatches()
    {
        Base().Equals(Base()).ShouldBeTrue();
        Base().Equals(null).ShouldBeFalse();
    }

    [Fact]
    public void ReadMissingArgumentsAsNoArgumentsFromEitherSide()
    {
        new EditorSetting("VsCode", Arguments: null).Equals(new EditorSetting("VsCode", Arguments: [])).ShouldBeTrue();
        new EditorSetting("VsCode", Arguments: []).Equals(new EditorSetting("VsCode", Arguments: null)).ShouldBeTrue();
    }

    private static EditorSetting Base() => new("Custom", @"C:\Tools\subl.exe", ["-n", "{folder}"]);
}