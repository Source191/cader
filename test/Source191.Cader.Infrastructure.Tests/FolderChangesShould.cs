using Source191.Cader.Adapters.Files;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using System.Collections.Concurrent;

namespace Source191.Cader.Infrastructure.Tests;

/// <summary>The real watcher behind IFolderChanges, on a throwaway folder.</summary>
[Trait("Category", "Integration")]
public sealed class FolderChangesShould : IDisposable
{
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan Quiet = TimeSpan.FromMilliseconds(500);

    private readonly string root = Path.Combine(Path.GetTempPath(), "test-changes-" + Guid.NewGuid().ToString("N"));
    private readonly FolderChanges changes = new();
    private readonly ConcurrentQueue<FolderChange> reported = new();

    public FolderChangesShould()
    {
        Directory.CreateDirectory(root);
    }

    [Fact]
    public async Task ReportACreatedSubfolderAndFileWithTheirPathsInsideTheFolder()
    {
        using IDisposable watch = changes.Watch(FolderPath.From(root), subfolders: true, reported.Enqueue);

        Directory.CreateDirectory(Path.Combine(root, "aba4b38e"));
        await File.WriteAllTextAsync(Path.Combine(root, "aba4b38e", "state.json"), "{}", TestContext.Current.CancellationToken);

        await UntilAsync(() => reported.Any(change => change.Path == @"aba4b38e\state.json"));
        reported.ShouldContain(new FolderChange("aba4b38e", FolderChangeKind.Created));
        reported.ShouldContain(new FolderChange(@"aba4b38e\state.json", FolderChangeKind.Created));
    }

    [Fact]
    public async Task ReportARenameByItsNewName()
    {
        string temporary = Path.Combine(root, "state.json.tmp.4f0d6ab5");
        await File.WriteAllTextAsync(temporary, "{}", TestContext.Current.CancellationToken);
        using IDisposable watch = changes.Watch(FolderPath.From(root), subfolders: false, reported.Enqueue);

        File.Move(temporary, Path.Combine(root, "state.json"));

        await UntilAsync(() => reported.Contains(new FolderChange("state.json", FolderChangeKind.Renamed)));
    }

    [Fact]
    public async Task LeaveOutSubfoldersWhenNotAsked()
    {
        Directory.CreateDirectory(Path.Combine(root, "Northwind.Orders"));
        using IDisposable watch = changes.Watch(FolderPath.From(root), subfolders: false, reported.Enqueue);

        await File.WriteAllTextAsync(Path.Combine(root, "Northwind.Orders", "README.md"), "# Orders", TestContext.Current.CancellationToken);
        Directory.CreateDirectory(Path.Combine(root, "Northwind.Billing"));

        await UntilAsync(() => reported.Contains(new FolderChange("Northwind.Billing", FolderChangeKind.Created)));
        reported.ShouldNotContain(change => change.Path.Contains("README", StringComparison.Ordinal));
    }

    [Fact]
    public async Task AnswerAWatchThatReportsNothingForAMissingFolder()
    {
        string missing = Path.Combine(root, "sessions");

        using IDisposable watch = changes.Watch(FolderPath.From(missing), subfolders: false, reported.Enqueue);
        Directory.CreateDirectory(missing);
        await File.WriteAllTextAsync(Path.Combine(missing, "4242.json"), "{}", TestContext.Current.CancellationToken);

        await Task.Delay(Quiet, TestContext.Current.CancellationToken);
        reported.ShouldBeEmpty();
    }

    [Fact]
    public async Task ReportNothingOnceDisposed()
    {
        IDisposable watch = changes.Watch(FolderPath.From(root), subfolders: true, reported.Enqueue);
        watch.Dispose();

        await File.WriteAllTextAsync(Path.Combine(root, "4242.json"), "{}", TestContext.Current.CancellationToken);

        await Task.Delay(Quiet, TestContext.Current.CancellationToken);
        reported.ShouldBeEmpty();
    }

    public void Dispose()
    {
        Directory.Delete(root, recursive: true);
    }

    private static async Task UntilAsync(Func<bool> condition)
    {
        DateTimeOffset giveUp = DateTimeOffset.UtcNow + Patience;
        while (!condition())
        {
            DateTimeOffset.UtcNow.ShouldBeLessThan(giveUp, "the change was never reported");
            await Task.Delay(TimeSpan.FromMilliseconds(20), TestContext.Current.CancellationToken);
        }
    }
}