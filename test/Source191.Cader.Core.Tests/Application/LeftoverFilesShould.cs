using Source191.Cader.Application;
using Source191.Cader.Domain;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application;

public sealed class LeftoverFilesShould
{
    private const string Leftover = @"C:\Users\me\source\worktrees\Northwind.Orders-fix-123";

    private static readonly FolderPath Folder = FolderPath.From(Leftover);

    [Fact]
    public void CountNothingInAFolderThatIsGone()
    {
        LeftoverFiles.Count(new FakeFolders(), Folder).ShouldBe(0);
    }

    [Fact]
    public void CountEveryFileInsideTheFolderExceptTheGitPointer()
    {
        FakeFolders folders = new FakeFolders()
            .WithFile($@"{Leftover}\.git")
            .WithFile($@"{Leftover}\notes.txt")
            .WithFile($@"{Leftover}\src\Orders.cs")
            .WithFile($@"{Leftover}\src\.GIT");

        LeftoverFiles.Count(folders, Folder).ShouldBe(2, "the pointer file counts nowhere, however deep or however it is cased");
    }

    [Fact]
    public void StopCountingAtTheLimit()
    {
        FakeFolders folders = new();
        foreach (int number in Enumerable.Range(1, WorktreeRemovalFacts.LeftoverFileLimit + 5))
        {
            folders.WithFile($@"{Leftover}\file-{number}.txt");
        }

        LeftoverFiles.Count(folders, Folder).ShouldBe(WorktreeRemovalFacts.LeftoverFileLimit);
    }
}