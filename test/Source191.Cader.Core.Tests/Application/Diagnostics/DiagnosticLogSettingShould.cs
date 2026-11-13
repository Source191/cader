using Source191.Cader.Application.Diagnostics;

namespace Source191.Cader.Core.Tests.Application.Diagnostics;

public sealed class DiagnosticLogSettingShould
{
    [Fact]
    public void ReadAPathAsAFile()
    {
        DiagnosticLogSetting.Read(@"C:\Users\me\cader.log").ShouldBe(new DiagnosticLogSetting.File(@"C:\Users\me\cader.log"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ReadNothingOrBlankAsOff(string? variable)
    {
        DiagnosticLogSetting.Read(variable).ShouldBe(new DiagnosticLogSetting.Off());
    }
}