using Source191.Cader.Adapters.Processes;
using System.Text;
using System.Text.RegularExpressions;

namespace Source191.Cader.Infrastructure.Tests;

public sealed partial class StartProgramScriptShould
{
    private const string Executable = @"C:\Users\me\AppData\Local\Programs\Microsoft VS Code\Code.exe";

    [Theory]
    [InlineData(@"C:\Users\me\source\repos\Northwind & Co's tools", "--new-window")]
    [InlineData(@"C:\Users\me\100% ^ (a)\[1]\é☃", "Northwind;\"x\"")]
    public void HoldNoPartOfTheExecutableOrArgumentsInClear(string folder, string option)
    {
        string script = StartProgramScript.For(Executable, [folder, option]);

        script.ShouldNotContain("Northwind");
        script.ShouldNotContain("Code.exe");
        script.ShouldNotContain("&");
        script.ShouldNotContain("%");
        script.Count(character => character == '\'').ShouldBe(6, "the only quotes are around the three encoded values");
    }

    [Theory]
    [InlineData(@"C:\Users\me\source\repos\Northwind & Co's tools", "--new-window")]
    [InlineData(@"C:\Users\me\100% ^ (a)\[1]\é☃", "Northwind;\"x\"")]
    public void DecodeToTheSameExecutableAndArgumentsInOrder(string folder, string option)
    {
        string script = StartProgramScript.For(Executable, [folder, option]);

        string[] decoded = [.. EncodedValue().Matches(script).Select(match => Encoding.UTF8.GetString(Convert.FromBase64String(match.Groups["text"].Value)))];
        decoded.ShouldBe([Executable, folder, option]);
        script.ShouldContain("$info.ArgumentList.Add(");
    }

    [Fact]
    public void StartThroughShellExecute()
    {
        string script = StartProgramScript.For(Executable, [@"C:\Users\me\source\repos\Northwind.Orders"]);

        script.ShouldContain("$info.UseShellExecute = $true");
        script.ShouldContain("[System.Diagnostics.Process]::Start($info) | Out-Null");
    }

    [Fact]
    public void ReportARefusalAsItsNativeErrorCodeAlone()
    {
        string script = StartProgramScript.For(Executable, [@"C:\Users\me\source\repos\Northwind.Orders"]);

        script.ShouldContain("} catch [System.ComponentModel.Win32Exception] {\n    [Console]::Error.WriteLine($_.Exception.NativeErrorCode)\n    exit 1\n}");
    }

    [Fact]
    public void ReportAnyOtherFailureOnStandardErrorAndExitOne()
    {
        string script = StartProgramScript.For(Executable, [@"C:\Users\me\source\repos\Northwind.Orders"]);

        script.ShouldEndWith("} catch {\n    [Console]::Error.WriteLine($_.Exception.Message)\n    exit 1\n}");
    }

    [GeneratedRegex(@"FromBase64String\('(?<text>[A-Za-z0-9+/=]*)'\)")]
    private static partial Regex EncodedValue();
}