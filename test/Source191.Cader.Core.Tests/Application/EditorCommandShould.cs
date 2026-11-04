using Source191.Cader.Application;
using Source191.Cader.Domain;

namespace Source191.Cader.Core.Tests.Application;

public sealed class EditorCommandShould
{
    [Fact]
    public void PassTheFolderAsOneArgument()
    {
        FolderPath folder = FolderPath.From(@"C:\Users\me\source\repos\Northwind & Co's ""tools""");

        Ports.ProcessRequest request = EditorCommand.For(EditorChoice.VsCode, @"C:\Code\Code.exe", folder);

        request.FileName.ShouldBe(@"C:\Code\Code.exe");
        request.Arguments.ShouldBe([folder.Value]);
        request.WorkingDirectory.ShouldBe(Ports.WorkingFolder.Inherit, "an editor starts in Cader's folder, as before");
    }

    [Fact]
    public void ReplaceFolderInsideACustomArgument()
    {
        EditorChoice editor = EditorChoice.Custom(@"C:\Tools\subl.exe", ["-n", "--folder={folder}", "--wait"]);
        FolderPath folder = FolderPath.From(@"C:\Users\me\source\repos\Northwind.Orders");

        Ports.ProcessRequest request = EditorCommand.For(editor, editor.Executable, folder);

        request.FileName.ShouldBe(@"C:\Tools\subl.exe");
        request.Arguments.ShouldBe(["-n", @"--folder=C:\Users\me\source\repos\Northwind.Orders", "--wait"]);
        request.WorkingDirectory.ShouldBe(Ports.WorkingFolder.Inherit);
    }
}