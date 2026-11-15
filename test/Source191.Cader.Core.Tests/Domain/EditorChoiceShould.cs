using Source191.Cader.Domain;

namespace Source191.Cader.Core.Tests.Domain;

public sealed class EditorChoiceShould
{
    private static readonly Func<string, bool> Installed = path => path == @"C:\Tools\subl.exe";

    [Fact]
    public void OfferVisualStudioInPlaceOfCursor()
    {
        Enum.GetValues<EditorPreset>().ShouldBe([EditorPreset.VsCode, EditorPreset.VisualStudio, EditorPreset.Rider, EditorPreset.Explorer, EditorPreset.Custom], "the owner's choice of 31 October 2026");
    }

    [Fact]
    public void DefaultToVsCode()
    {
        EditorChoice.FromFile(preset: null, executable: null, arguments: null).ShouldBe(EditorChoice.VsCode);
    }

    [Fact]
    public void KeepACustomEditorWithAFolderPlaceholder()
    {
        EditorChoice editor = EditorChoice.FromFile(EditorPreset.Custom, @"C:\Tools\subl.exe", ["-n", "{folder}"]);

        editor.ShouldBe(EditorChoice.Custom(@"C:\Tools\subl.exe", ["-n", "{folder}"]));
        editor.Executable.ShouldBe(@"C:\Tools\subl.exe");
        editor.Arguments.ShouldBe(["-n", "{folder}"]);
    }

    [Fact]
    public void ReadACustomEditorWithoutAPlaceholderAsVsCode()
    {
        EditorChoice.FromFile(EditorPreset.Custom, @"C:\Tools\subl.exe", ["-n"]).ShouldBe(EditorChoice.VsCode);
    }

    [Fact]
    public void ReadACustomEditorWithNoArgumentsAsVsCode()
    {
        EditorChoice.FromFile(EditorPreset.Custom, @"C:\Tools\subl.exe", arguments: null).ShouldBe(EditorChoice.VsCode);
    }

    [Fact]
    public void ReadACustomEditorWithoutAnExecutableAsVsCode()
    {
        EditorChoice.FromFile(EditorPreset.Custom, executable: null, ["{folder}"]).ShouldBe(EditorChoice.VsCode);
    }

    [Fact]
    public void ReadACustomEditorByBareNameAsVsCode()
    {
        EditorChoice.FromFile(EditorPreset.Custom, "subl", ["{folder}"]).ShouldBe(EditorChoice.VsCode);
    }

    [Fact]
    public void ReadACustomBatchFileEditorAsVsCode()
    {
        EditorChoice.FromFile(EditorPreset.Custom, @"C:\Tools\edit.cmd", ["{folder}"]).ShouldBe(EditorChoice.VsCode);
    }

    [Fact]
    public void RefuseACustomCommandWithTheFirstProblem()
    {
        EditorChoice.Validate("  ", ["-n"], Installed).ShouldBe(new EditorChoiceValidation.Refused(new SettingsProblem.NoExecutable()));
        EditorChoice.Validate("subl.cmd", ["-n"], Installed).ShouldBe(new EditorChoiceValidation.Refused(new SettingsProblem.ExecutableNotFullPath()));
        EditorChoice.Validate(@"C:\Tools\edit.cmd", ["-n"], Installed).ShouldBe(new EditorChoiceValidation.Refused(new SettingsProblem.BatchFileExecutable()));
        EditorChoice.Validate(@"C:\Tools\subl.exe", ["-n"], Installed).ShouldBe(new EditorChoiceValidation.Refused(new SettingsProblem.NoFolderPlaceholder()));
    }

    [Fact]
    public void RefuseAnExecutableThatDoesNotExist()
    {
        EditorChoice.Validate(@" C:\Tools\subl.exe ", ["-n", "{folder}"], _ => false).ShouldBe(new EditorChoiceValidation.Refused(new SettingsProblem.ExecutableMissing(@"C:\Tools\subl.exe")));
        EditorChoice.Validate("subl.exe", ["{folder}"], _ => false).ShouldBe(new EditorChoiceValidation.Refused(new SettingsProblem.ExecutableNotFullPath()), "the typed text's own reason wins");
        EditorChoice.Validate(@"C:\Tools\edit.bat", ["{folder}"], _ => false).ShouldBe(new EditorChoiceValidation.Refused(new SettingsProblem.BatchFileExecutable()), "the typed text's own reason wins");
        EditorChoice.Validate(@"C:\Tools\subl.exe", ["-n"], _ => false).ShouldBe(new EditorChoiceValidation.Refused(new SettingsProblem.NoFolderPlaceholder()), "the typed text's own reason wins");
    }

    [Fact]
    public void KeepACustomEditorFromTheFileWhoseExecutableIsMissing()
    {
        EditorChoice editor = EditorChoice.FromFile(EditorPreset.Custom, @"E:\Tools\subl.exe", ["{folder}"]);

        editor.Preset.ShouldBe(EditorPreset.Custom);
        editor.Executable.ShouldBe(@"E:\Tools\subl.exe");
    }

    [Fact]
    public void AcceptACustomCommandAsValid()
    {
        EditorChoice editor = EditorChoice.Validate(@" C:\Tools\subl.exe ", ["-n", "{folder}"], Installed).ShouldBeOfType<EditorChoiceValidation.Valid>().Editor;

        editor.Preset.ShouldBe(EditorPreset.Custom);
        editor.Executable.ShouldBe(@"C:\Tools\subl.exe");
        editor.Arguments.ShouldBe(["-n", "{folder}"]);
    }

    [Theory]
    [InlineData(EditorPreset.VsCode)]
    [InlineData(EditorPreset.VisualStudio)]
    [InlineData(EditorPreset.Rider)]
    [InlineData(EditorPreset.Explorer)]
    public void KeepAPresetWithoutAnyCommand(EditorPreset preset)
    {
        EditorChoice editor = EditorChoice.FromFile(preset, @"C:\Tools\subl.exe", ["{folder}"]);

        editor.ShouldBe(EditorChoice.Of(preset));
        editor.Executable.ShouldBeEmpty();
        editor.Arguments.ShouldBeEmpty();
    }

    [Fact]
    public void ReportTheCommandAndWhyACustomEditorIsRefused()
    {
        ArgumentException refused = Should.Throw<ArgumentException>(() => EditorChoice.Custom(@"C:\Tools\subl.exe", ["-n"]));

        refused.Message.ShouldContain(@"'C:\Tools\subl.exe' is not a usable custom editor");
        refused.Message.ShouldContain(nameof(SettingsProblem.NoFolderPlaceholder));
    }

    public static TheoryData<EditorChoice, EditorChoice> DifferentChoices => new()
    {
        { EditorChoice.Of(EditorPreset.VsCode), EditorChoice.Of(EditorPreset.Rider) },
        { EditorChoice.Custom(@"C:\Tools\subl.exe", ["{folder}"]), EditorChoice.Custom(@"C:\Tools\edit.exe", ["{folder}"]) },
        { EditorChoice.Custom(@"C:\Tools\subl.exe", ["-n", "{folder}"]), EditorChoice.Custom(@"C:\Tools\subl.exe", ["-w", "{folder}"]) },
    };

    [Theory]
    [MemberData(nameof(DifferentChoices))]
    public void BeUnequalWhenAnyPartDiffers(EditorChoice first, EditorChoice second)
    {
        first.Equals(second).ShouldBeFalse();
        second.Equals(first).ShouldBeFalse();
    }

    [Fact]
    public void BeEqualWhenEveryPartMatches()
    {
        EditorChoice.Custom(@"C:\Tools\subl.exe", ["-n", "{folder}"]).Equals(EditorChoice.Custom(@"C:\Tools\subl.exe", ["-n", "{folder}"])).ShouldBeTrue();
        EditorChoice.Of(EditorPreset.Rider).Equals(null).ShouldBeFalse();
    }
}