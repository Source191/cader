namespace Source191.Cader.HeadlessTerminal.Tests;

public sealed class CommandLineShould
{
    [Fact]
    public void LeavePlainArgumentsAloneAndQuoteTheOnesWithSpacesOrQuotes()
    {
        string line = CommandLine.Build(@"C:\tools\cader.exe", ["here", "fix tests", "-p", "say \"hi\""]);

        line.ShouldBe(@"C:\tools\cader.exe here ""fix tests"" -p ""say \""hi\""""");
    }

    [Fact]
    public void DoubleTrailingBackslashesInsideQuotes()
    {
        CommandLine.Quote(@"C:\a folder\").ShouldBe(@"""C:\a folder\\""");
    }
}