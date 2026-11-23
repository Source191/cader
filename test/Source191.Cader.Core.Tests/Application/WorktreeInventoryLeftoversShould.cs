using Microsoft.Extensions.Time.Testing;
using Source191.Cader.Adapters.Git;
using Source191.Cader.Application;
using Source191.Cader.Application.Diagnostics;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application;

/// <summary>The leftover cases over an in-memory folder tree; the registered-worktree cases run against real git in WorktreeInventoryShould.</summary>
public sealed class WorktreeInventoryLeftoversShould : IDisposable
{
    private const string Root = @"C:\Users\me\source\repos";
    private const string Repository = @"C:\Users\me\source\repos\Northwind.Orders";
    private const string Billing = @"C:\Users\me\source\repos\Northwind.Billing";
    private const string WorktreeRoot = @"C:\Users\me\source\worktrees";

    private static readonly string[] TenNames = ["Billing", "Customers", "Inventory", "Ledger", "Orders", "Payroll", "Products", "Shipping", "Suppliers", "Warehouse"];

    private readonly FakeFolders folders = new FakeFolders().WithDirectory(Path.Combine(Repository, ".git", "objects"));
    private readonly FakeProcessRunner processes = new FakeProcessRunner()
        .AnswerAll(new ProcessResult(0, string.Empty, string.Empty))
        .Answer(request => request.Arguments.Contains("--porcelain"), new ProcessResult(0, "worktree C:/Users/me/source/repos/Northwind.Orders\nHEAD a\nbranch refs/heads/main\n", string.Empty));

    private readonly PullRequestCatalogue catalogue = new(new GitCli(new FakeProcessRunner().AnswerAll(new ProcessResult(2, string.Empty, string.Empty)), "git"), new FakePullRequests(), new FakeTimeProvider(Fixtures.Noon), new RefreshCounts(new FakeTimeProvider(Fixtures.Noon), new DiagnosticLog(new FakeTimeProvider(Fixtures.Noon), [])));

    private readonly DiagnosticLog log = new(new FakeTimeProvider(Fixtures.Noon), []);

    private WorktreeInventory Inventory => new(new GitCli(processes, "git"), folders, TestSettings.Current(Root), catalogue, new OriginPruner(new GitCli(processes, "git"), new FakeTimeProvider(Fixtures.Noon), log), log);

    [Fact]
    public async Task AskGitThreeThingsPerRegisteredWorktreeAndNothingAboutMergeState()
    {
        string worktree = Path.Combine(Root, "Northwind.Orders-fix-123");
        folders.WithDirectory(worktree);
        processes.Answer(request => request.Arguments.Contains("--porcelain"), new ProcessResult(0, "worktree C:/Users/me/source/repos/Northwind.Orders\nHEAD a\nbranch refs/heads/main\n\nworktree C:/Users/me/source/repos/Northwind.Orders-fix-123\nHEAD b\nbranch refs/heads/fix-123\n", string.Empty));

        processes.Answer(request => request.Arguments.Contains("rev-list"), new ProcessResult(0, "0\n", string.Empty));

        await Inventory.ScanAsync([], CancellationToken.None);

        processes.CapturedRequests.Where(request => request.WorkingDirectory == new WorkingFolder.At(worktree)).Select(request => request.Arguments[0]).ShouldBe(["status", "rev-list", "log"]);
        processes.CapturedRequests.ShouldNotContain(request => request.Arguments.Contains("merge-base") || request.Arguments.Contains("symbolic-ref"));
    }

    [Fact]
    public async Task RecordAnUnreadableRepositoryOnceAndStillAnswerTheOtherRows()
    {
        string orphan = Path.Combine(Repository, ".claude", "worktrees", "fix-service-tests");
        folders.WithFile(Path.Combine(orphan, "Program.cs"));
        processes.Answer(request => request.Arguments.Contains("--porcelain"), new ProcessResult(128, string.Empty, "fatal: not a git repository"));

        WorktreeInventory inventory = Inventory;

        await inventory.ScanAsync([], CancellationToken.None);
        IReadOnlyList<WorktreeRow> second = await inventory.ScanAsync([], CancellationToken.None);

        second.Single().Health.Kind.ShouldBe(WorktreeHealthKind.Orphan);
        Failed failed = log.Snapshot().Single().Event.ShouldBeOfType<Failed>();
        failed.Site.ShouldBe(FailureSite.Load);
        failed.What.ShouldBe(CodeName.For(typeof(WorktreeInventory)));
    }

    [Fact]
    public async Task ReportAFolderUnderClaudeWorktreesThatGitNoLongerListsAsAnOrphanWithItsFileCount()
    {
        string orphan = Path.Combine(Repository, ".claude", "worktrees", "fix-service-tests");
        folders.WithFile(Path.Combine(orphan, "Program.cs")).WithFile(Path.Combine(orphan, "src", "Tests.cs"));

        WorktreeRow row = (await Inventory.ScanAsync([], CancellationToken.None)).Single();

        row.Path.Value.ShouldBe(orphan);
        row.Health.Kind.ShouldBe(WorktreeHealthKind.Orphan);
        row.LeftoverFileCount.ShouldBe(2);
        row.OwnerExists.ShouldBeTrue();
        row.PullRequest.ShouldBe(new PullRequestLookup.Missing(PullRequestGap.NoBranch), "a leftover has no branch to look up");
    }

    [Fact]
    public async Task ReportAnOrphanWhosePointerNamesAGoneRegistrationAsBrokenWithoutCountingThePointer()
    {
        string orphan = Path.Combine(Repository, ".claude", "worktrees", "fix-service-tests");
        folders.WithFile(Path.Combine(orphan, "Program.cs")).WithFile(Path.Combine(orphan, ".git"), "gitdir: C:/gone");

        WorktreeRow row = (await Inventory.ScanAsync([], CancellationToken.None)).Single();

        row.Health.Kind.ShouldBe(WorktreeHealthKind.Broken);
        row.LeftoverFileCount.ShouldBe(1, "the .git pointer file does not count");
    }

    [Fact]
    public async Task KeepAnOrphanWhosePointerNamesARegistrationThatStillExistsAnOrphan()
    {
        string orphan = Path.Combine(Repository, ".claude", "worktrees", "fix-service-tests");
        folders.WithFile(Path.Combine(orphan, ".git"), "gitdir: C:/Users/me/source/repos/Northwind.Orders/.git/worktrees/fix-service-tests\n")
            .WithDirectory(Path.Combine(Repository, ".git", "worktrees", "fix-service-tests"));

        WorktreeRow row = (await Inventory.ScanAsync([], CancellationToken.None)).Single();

        row.Health.ShouldBe(new WorktreeHealth(WorktreeHealthKind.Orphan, new HealthReason.UnderClaudeWorktrees()), "an orphan never names an owner, so it is never elsewhere");
    }

    [Fact]
    public async Task ReportAnEmptyWorktreesFolderDirectlyUnderARootAsEmpty()
    {
        folders.WithDirectory(Path.Combine(Root, "Northwind.Orders.worktrees")).WithDirectory(Path.Combine(Root, "just-a-folder"));

        WorktreeRow row = (await Inventory.ScanAsync([], CancellationToken.None)).Single();

        row.FolderName.ShouldBe("Northwind.Orders.worktrees");
        row.Health.Kind.ShouldBe(WorktreeHealthKind.Empty);
        row.Repository.Value.ShouldBe(Root);
    }

    [Fact]
    public async Task ReportASiblingWhosePointerNamesAGoneRegistrationAsBrokenWithItsOwner()
    {
        string sibling = Path.Combine(Root, "Northwind.Orders-republish");
        folders.WithFile(Path.Combine(sibling, ".git"), "gitdir: C:/Users/me/source/repos/Northwind.Orders/.git/worktrees/republish\n").WithFile(Path.Combine(sibling, "notes.txt"));

        WorktreeRow row = (await Inventory.ScanAsync([], CancellationToken.None)).Single();

        row.Path.Value.ShouldBe(sibling);
        row.Health.Kind.ShouldBe(WorktreeHealthKind.Broken);
        row.Repository.Value.ShouldBe(Repository);
        row.OwnerExists.ShouldBeTrue();
        row.LeftoverFileCount.ShouldBe(1);
    }

    [Fact]
    public async Task ReportAWorktreeOfARepositoryOutsideTheRootsAsElsewhere()
    {
        string worktree = Path.Combine(WorktreeRoot, "Northwind.Ledger-fix");
        folders.WithFile(Path.Combine(worktree, ".git"), "gitdir: D:/other/Northwind.Ledger/.git/worktrees/fix\n")
            .WithDirectory(@"D:\other\Northwind.Ledger\.git\worktrees\fix")
            .WithFile(Path.Combine(worktree, "notes.txt"));

        WorktreeRow row = (await Inventory.ScanAsync([], CancellationToken.None)).Single();

        row.Health.Kind.ShouldBe(WorktreeHealthKind.Elsewhere);
        row.Health.Reason.ShouldBe(new HealthReason.RepositoryOutsideRoots(FolderPath.From(@"D:\other\Northwind.Ledger")));
        row.Repository.Value.ShouldBe(@"D:\other\Northwind.Ledger");
        row.OwnerExists.ShouldBeTrue();
    }

    [Fact]
    public async Task FindABrokenWorktreeUnderTheWorktreeRoot()
    {
        string worktree = Path.Combine(WorktreeRoot, "Northwind.Orders-republish");
        folders.WithFile(Path.Combine(worktree, ".git"), "gitdir: C:/Users/me/source/repos/Northwind.Orders/.git/worktrees/republish\n");

        WorktreeRow row = (await Inventory.ScanAsync([], CancellationToken.None)).Single();

        row.Path.Value.ShouldBe(worktree);
        row.Health.Kind.ShouldBe(WorktreeHealthKind.Broken);
        row.Repository.Value.ShouldBe(Repository);
    }

    [Fact]
    public async Task ListAnEmptyFolderUnderTheWorktreeRootAsEmpty()
    {
        string folder = Path.Combine(WorktreeRoot, "Northwind.Orders-fix-123");
        folders.WithDirectory(folder);

        WorktreeRow row = (await Inventory.ScanAsync([], CancellationToken.None)).Single();

        row.Path.Value.ShouldBe(folder);
        row.Health.Kind.ShouldBe(WorktreeHealthKind.Empty);
        row.Repository.Value.ShouldBe(WorktreeRoot);
        row.OwnerExists.ShouldBeFalse();
        row.LeftoverFileCount.ShouldBe(0);
    }

    [Fact]
    public async Task SkipAFolderUnderTheWorktreeRootThatHoldsFilesButNoGitFile()
    {
        folders.WithFile(Path.Combine(WorktreeRoot, "Northwind.Notes", "notes.txt"));

        (await Inventory.ScanAsync([], CancellationToken.None)).ShouldBeEmpty();
    }

    [Fact]
    public async Task LeaveAnEmptyFolderGitStillRegistersToItsRegisteredRow()
    {
        string folder = Path.Combine(WorktreeRoot, "Northwind.Orders-fix-123");
        folders.WithDirectory(folder);
        processes.Answer(request => request.Arguments.Contains("--porcelain"), new ProcessResult(0, "worktree C:/Users/me/source/repos/Northwind.Orders\nHEAD a\nbranch refs/heads/main\n\nworktree C:/Users/me/source/worktrees/Northwind.Orders-fix-123\nHEAD b\nbranch refs/heads/fix-123\nprunable gitdir file points to non-existent location\n", string.Empty));

        WorktreeRow row = (await Inventory.ScanAsync([], CancellationToken.None)).Single();

        row.Health.Kind.ShouldBe(WorktreeHealthKind.Broken);
        row.Health.Reason.ShouldBe(new HealthReason.GitFileGone());
        row.Repository.Value.ShouldBe(Repository);
    }

    [Fact]
    public async Task ListNoEmptyRowsWhenTheWorktreeRootIsARepositoryRoot()
    {
        folders.WithDirectory(Path.Combine(Root, "Northwind.Billing"));
        WorktreeInventory inventory = new(new GitCli(processes, "git"), folders, TestSettings.Current(TestSettings.For([Root], worktreeRoot: Root)), catalogue, new OriginPruner(new GitCli(processes, "git"), new FakeTimeProvider(Fixtures.Noon), log), log);

        (await inventory.ScanAsync([], CancellationToken.None)).ShouldBeEmpty();
    }

    [Fact]
    public async Task NotListAFolderTwiceWhenTheWorktreeRootIsARoot()
    {
        string sibling = Path.Combine(Root, "Northwind.Orders-republish");
        folders.WithFile(Path.Combine(sibling, ".git"), "gitdir: C:/Users/me/source/repos/Northwind.Orders/.git/worktrees/republish\n");
        WorktreeInventory inventory = new(new GitCli(processes, "git"), folders, TestSettings.Current(TestSettings.For([Root], worktreeRoot: Root)), catalogue, new OriginPruner(new GitCli(processes, "git"), new FakeTimeProvider(Fixtures.Noon), log), log);

        (await inventory.ScanAsync([], CancellationToken.None)).Select(row => row.Path.Value).ShouldBe([sibling]);
    }

    [Fact]
    public async Task ScanTheRootsSavedAfterItWasBuilt()
    {
        const string Work = @"D:\work";
        string sibling = Path.Combine(Work, "Northwind.Ledger-republish");
        folders.WithFile(Path.Combine(sibling, ".git"), "gitdir: D:/work/Northwind.Ledger/.git/worktrees/republish\n");
        CurrentSettings settings = TestSettings.Current(Root);
        WorktreeInventory inventory = new(new GitCli(processes, "git"), folders, settings, catalogue, new OriginPruner(new GitCli(processes, "git"), new FakeTimeProvider(Fixtures.Noon), log), log);

        await settings.UpdateAsync(_ => TestSettings.For(Root, Work));

        (await inventory.ScanAsync([], CancellationToken.None)).Select(row => row.Path.Value).ShouldBe([sibling]);
    }

    [Fact]
    public async Task ListLeftoversUnderARepositoryRootInNameOrder()
    {
        folders.WithDirectory(Path.Combine(Root, "Northwind.Ledger.worktrees")).WithDirectory(Path.Combine(Root, "Northwind.Billing.worktrees"));

        IReadOnlyList<WorktreeRow> rows = await Inventory.ScanAsync([], CancellationToken.None);

        rows.Select(row => row.FolderName).ShouldBe(["Northwind.Billing.worktrees", "Northwind.Ledger.worktrees"]);
    }

    [Fact]
    public async Task ListEmptyFoldersUnderTheWorktreeRootInNameOrder()
    {
        folders.WithDirectory(Path.Combine(WorktreeRoot, "Northwind.Orders-zeta")).WithDirectory(Path.Combine(WorktreeRoot, "Northwind.Orders-alpha"));

        IReadOnlyList<WorktreeRow> rows = await Inventory.ScanAsync([], CancellationToken.None);

        rows.Select(row => row.FolderName).ShouldBe(["Northwind.Orders-alpha", "Northwind.Orders-zeta"]);
    }

    [Fact]
    public async Task NotListAWorktreeUnderClaudeWorktreesAsAnOrphanWhenGitRegistersIt()
    {
        string worktree = Path.Combine(Repository, ".claude", "worktrees", "fix-123");
        folders.WithDirectory(worktree);
        processes.Answer(request => request.Arguments.Contains("--porcelain") && request.Arguments[0] == "worktree", new ProcessResult(0, "worktree C:/Users/me/source/repos/Northwind.Orders\nHEAD a\nbranch refs/heads/main\n\nworktree C:/Users/me/source/repos/Northwind.Orders/.claude/worktrees/fix-123\nHEAD b\nbranch refs/heads/fix-123\n", string.Empty));

        WorktreeRow row = (await Inventory.ScanAsync([], CancellationToken.None)).Single();

        row.Path.Value.ShouldBe(worktree);
        row.Health.Kind.ShouldBe(WorktreeHealthKind.Ok);
    }

    [Fact]
    public async Task ListOrphansInNameOrder()
    {
        string orphans = Path.Combine(Repository, ".claude", "worktrees");
        folders.WithFile(Path.Combine(orphans, "fix-b", "Program.cs")).WithFile(Path.Combine(orphans, "fix-a", "Program.cs"));

        IReadOnlyList<WorktreeRow> rows = await Inventory.ScanAsync([], CancellationToken.None);

        rows.Select(row => row.FolderName).ShouldBe(["fix-a", "fix-b"]);
    }

    [Fact]
    public async Task AskForThePullRequestOfEachRegisteredBranch()
    {
        folders.WithDirectory(Path.Combine(Root, "Northwind.Orders-fix-123"));
        AnswerListingWithOneWorktree();
        FakePullRequests hosts = new();
        FakeProcessRunner origin = new FakeProcessRunner().Answer(request => request.Arguments.SequenceEqual(["remote", "get-url", "origin"]), new ProcessResult(0, "git@github.com:northwind/Northwind.Orders.git\n", string.Empty));
        FakeTimeProvider clock = new(Fixtures.Noon);
        using PullRequestCatalogue lookups = new(new GitCli(origin, "git"), hosts, clock, new RefreshCounts(clock, log));
        WorktreeInventory inventory = new(new GitCli(processes, "git"), folders, TestSettings.Current(Root), lookups, new OriginPruner(new GitCli(processes, "git"), clock, log), log);

        await inventory.ScanAsync([], CancellationToken.None);
        await hosts.WhenCalledAsync(1).WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        hosts.Requests.ShouldBe([(new RemoteRepository.GitHub("northwind", "Northwind.Orders"), BranchName.From("fix-123"))]);
    }

    [Fact]
    public async Task CountTheChangesOfARegisteredWorktreeAndMarkItsOwnerAndFolderAsThere()
    {
        folders.WithDirectory(Path.Combine(Root, "Northwind.Orders-fix-123"));
        AnswerListingWithOneWorktree();
        processes.Answer(request => request.Arguments[0] == "status", new ProcessResult(0, " M Program.cs\n?? notes.txt\n", string.Empty));
        processes.Answer(request => request.Arguments[0] == "rev-list", new ProcessResult(0, "3\n", string.Empty));
        processes.Answer(request => request.Arguments[0] == "log", new ProcessResult(0, "2026-10-20T09:00:00+00:00\n", string.Empty));

        WorktreeRow row = (await Inventory.ScanAsync([], CancellationToken.None)).Single();

        row.Health.Kind.ShouldBe(WorktreeHealthKind.Ok);
        row.DirtyFiles.ShouldBe(2);
        row.UnpushedCommits.ShouldBe(3);
        row.LastCommitAt.ShouldBe(new LastCommit.Made(new DateTimeOffset(2026, 10, 20, 9, 0, 0, TimeSpan.Zero)));
        row.LeftoverFileCount.ShouldBe(0);
        row.OwnerExists.ShouldBeTrue();
        row.FolderExists.ShouldBeTrue();
    }

    [Fact]
    public async Task KeepARegisteredWorktreeGitCannotReadAsARowWithNoCounts()
    {
        folders.WithDirectory(Path.Combine(Root, "Northwind.Orders-fix-123"));
        AnswerListingWithOneWorktree();
        processes.Answer(request => request.Arguments[0] == "status", new ProcessResult(128, string.Empty, "fatal: not a git repository"));

        WorktreeRow row = (await Inventory.ScanAsync([], CancellationToken.None)).Single();

        row.Health.Kind.ShouldBe(WorktreeHealthKind.Ok);
        row.DirtyFiles.ShouldBe(0);
        row.LastCommitAt.ShouldBe(new LastCommit.Unknown());
        row.OwnerExists.ShouldBeTrue();
        row.FolderExists.ShouldBeTrue();
        log.Snapshot().Single().Event.ShouldBeOfType<Failed>();
    }

    [Fact]
    public async Task ReportARegisteredWorktreeWhoseFolderIsGoneWithoutAskingGitAboutIt()
    {
        string worktree = Path.Combine(Root, "Northwind.Orders-fix-123");
        AnswerListingWithOneWorktree();

        WorktreeRow row = (await Inventory.ScanAsync([], CancellationToken.None)).Single();

        row.Health.ShouldBe(new WorktreeHealth(WorktreeHealthKind.Broken, new HealthReason.FolderGone()));
        row.FolderExists.ShouldBeFalse();
        row.OwnerExists.ShouldBeTrue();
        processes.CapturedRequests.ShouldNotContain(request => request.WorkingDirectory == new WorkingFolder.At(worktree));
    }

    [Fact]
    public async Task CountTheFilesOfAPrunableWorktreeWithoutAskingGitAboutIt()
    {
        string worktree = Path.Combine(Root, "Northwind.Orders-fix-123");
        folders.WithFile(Path.Combine(worktree, "Program.cs"));
        processes.Answer(request => request.Arguments.Contains("--porcelain"), new ProcessResult(0, "worktree C:/Users/me/source/repos/Northwind.Orders\nHEAD a\nbranch refs/heads/main\n\nworktree C:/Users/me/source/repos/Northwind.Orders-fix-123\nHEAD b\nbranch refs/heads/fix-123\nprunable gitdir file points to non-existent location\n", string.Empty));

        WorktreeRow row = (await Inventory.ScanAsync([], CancellationToken.None)).Single();

        row.Health.ShouldBe(new WorktreeHealth(WorktreeHealthKind.Broken, new HealthReason.GitFileGone()));
        row.LeftoverFileCount.ShouldBe(1);
        row.FolderExists.ShouldBeTrue();
        row.OwnerExists.ShouldBeTrue();
        processes.CapturedRequests.ShouldNotContain(request => request.WorkingDirectory == new WorkingFolder.At(worktree));
    }

    [Fact]
    public async Task SkipARegisteredWorktreeWhosePathGitGaveIsNoFullPath()
    {
        processes.Answer(request => request.Arguments.Contains("--porcelain"), new ProcessResult(0, "worktree C:/Users/me/source/repos/Northwind.Orders\nHEAD a\nbranch refs/heads/main\n\nworktree relative/Northwind.Orders-fix-123\nHEAD b\nbranch refs/heads/fix-123\n", string.Empty));

        (await Inventory.ScanAsync([], CancellationToken.None)).ShouldBeEmpty();
    }

    [Fact]
    public async Task NameTheWorktreeRootAsTheOwnerOfABrokenWorktreeWhoseGitFileNamesNoFolder()
    {
        string folder = Path.Combine(WorktreeRoot, "Northwind.Orders-garbled");
        folders.WithFile(Path.Combine(folder, ".git"), "not a pointer\n");

        WorktreeRow row = (await Inventory.ScanAsync([], CancellationToken.None)).Single();

        row.Health.ShouldBe(new WorktreeHealth(WorktreeHealthKind.Broken, new HealthReason.RegistrationGone()));
        row.Repository.Value.ShouldBe(WorktreeRoot);
        row.OwnerExists.ShouldBeFalse();
    }

    [Fact]
    public async Task NameTheContainingFolderAsTheOwnerOfABrokenWorktreeWhoseGitDirIsNoRegistration()
    {
        string sibling = Path.Combine(Root, "Northwind.Orders-republish");
        folders.WithFile(Path.Combine(sibling, ".git"), "gitdir: C:/gone/Northwind.Orders\n");

        WorktreeRow row = (await Inventory.ScanAsync([], CancellationToken.None)).Single();

        row.Health.Kind.ShouldBe(WorktreeHealthKind.Broken);
        row.Repository.Value.ShouldBe(Root);
        row.OwnerExists.ShouldBeFalse();
    }

    [Fact]
    public async Task ReadARelativeGitDirFromTheFolderOfTheGitFile()
    {
        string worktree = Path.Combine(WorktreeRoot, "Northwind.Ledger-fix");
        folders.WithFile(Path.Combine(worktree, ".git"), "gitdir: ../../../../../other/Northwind.Ledger/.git/worktrees/fix\n")
            .WithDirectory(@"C:\other\Northwind.Ledger\.git\worktrees\fix");

        WorktreeRow row = (await Inventory.ScanAsync([], CancellationToken.None)).Single();

        row.Health.Kind.ShouldBe(WorktreeHealthKind.Elsewhere);
        row.Repository.Value.ShouldBe(@"C:\other\Northwind.Ledger");
    }

    [Fact]
    public async Task StillListTheOtherRepositoryWhenOneIsUnreadable()
    {
        string worktree = Path.Combine(Root, "Northwind.Billing-fix-123");
        folders.WithDirectory(Path.Combine(Billing, ".git", "objects")).WithDirectory(worktree);
        processes.Answer(request => IsListingOf(request, Repository), new ProcessResult(128, string.Empty, "fatal: not a git repository"));
        processes.Answer(request => IsListingOf(request, Billing), new ProcessResult(0, Listing(Billing, worktree), string.Empty));
        AnswerCounts(dirty: " M Program.cs\n", unpushed: "3\n");

        WorktreeRow row = (await Inventory.ScanAsync([], CancellationToken.None)).Single();

        row.Path.Value.ShouldBe(worktree);
        row.Repository.Value.ShouldBe(Billing);
        row.DirtyFiles.ShouldBe(1);
        row.UnpushedCommits.ShouldBe(3);
        log.Snapshot().Single().Event.ShouldBeOfType<Failed>();
    }

    [Fact]
    public async Task ListTwoRepositoriesAtOnceAndKeepTheirOrder()
    {
        string billingWorktree = Path.Combine(Root, "Northwind.Billing-fix-123");
        string ordersWorktree = Path.Combine(Root, "Northwind.Orders-fix-456");
        folders.WithDirectory(Path.Combine(Billing, ".git", "objects")).WithDirectory(billingWorktree).WithDirectory(ordersWorktree);
        processes.Answer(request => IsListingOf(request, Billing), new ProcessResult(0, Listing(Billing, billingWorktree), string.Empty));
        processes.Answer(request => IsListingOf(request, Repository), new ProcessResult(0, Listing(Repository, ordersWorktree), string.Empty));
        AnswerCounts(dirty: string.Empty, unpushed: "0\n");
        HeldRuns runs = new(IsListing);
        processes.WhileRunning = runs.WhileRunning;

        Task<IReadOnlyList<WorktreeRow>> scan = Inventory.ScanAsync([], CancellationToken.None);
        try
        {
            runs.CountStarted(IsListing).ShouldBe(2, "both repositories are listed at once");
            runs.Open(Repository);
            await runs.WhenStarted(request => request.Arguments[0] == "status" && request.WorkingDirectory == new WorkingFolder.At(ordersWorktree), 1).WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
            runs.Open(Billing);
        }
        finally
        {
            runs.OpenAll();
        }

        (await scan).Select(row => row.Path.Value).ShouldBe([billingWorktree, ordersWorktree], "Orders answered first, but Billing's rows still come first");
    }

    [Fact]
    public async Task InspectThreeWorktreesOfOneRepositoryAtOnce()
    {
        string[] worktrees = [Path.Combine(Root, "Northwind.Orders-fix-c"), Path.Combine(Root, "Northwind.Orders-fix-a"), Path.Combine(Root, "Northwind.Orders-fix-b")];
        processes.Answer(request => IsListingOf(request, Repository), new ProcessResult(0, Listing(Repository, worktrees), string.Empty));
        AnswerCounts(dirty: string.Empty, unpushed: "0\n");
        for (int index = 0; index < worktrees.Length; index++)
        {
            string worktree = worktrees[index];
            folders.WithDirectory(worktree);
            processes.Answer(request => IsStatus(request) && request.WorkingDirectory == new WorkingFolder.At(worktree), new ProcessResult(0, string.Concat(Enumerable.Repeat("?? notes.txt\n", index + 1)), string.Empty));
        }

        HeldRuns runs = new(IsStatus);
        processes.WhileRunning = runs.WhileRunning;

        Task<IReadOnlyList<WorktreeRow>> scan = Inventory.ScanAsync([], CancellationToken.None);
        try
        {
            await runs.WhenStarted(IsStatus, 3).WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        }
        finally
        {
            runs.OpenAll();
        }

        (await scan).Select(row => (row.Path.Value, row.DirtyFiles)).ShouldBe([(worktrees[0], 1), (worktrees[1], 2), (worktrees[2], 3)], "each row keeps its own counts, in git's listing order");
    }

    [Fact]
    public async Task StartAtMostEightGitReadsAtOnce()
    {
        string[] repositories = TenRepositories();
        HeldRuns runs = new(IsListing);
        processes.WhileRunning = runs.WhileRunning;

        Task<IReadOnlyList<WorktreeRow>> scan = Inventory.ScanAsync([], CancellationToken.None);
        try
        {
            runs.CountStarted(IsListing).ShouldBe(8);
        }
        finally
        {
            runs.OpenAll();
        }

        (await scan).Select(row => row.Repository.Value).ShouldBe(repositories, "every repository is still listed, in name order");
    }

    [Fact]
    public async Task StartNoGitReadOnceTheScanIsCancelled()
    {
        string[] repositories = TenRepositories();
        HeldRuns runs = new(IsListing);
        processes.WhileRunning = runs.WhileRunning;
        using CancellationTokenSource cancellation = new();

        Task<IReadOnlyList<WorktreeRow>> scan = Inventory.ScanAsync([], cancellation.Token);
        try
        {
            runs.CountStarted(IsListing).ShouldBe(8);
            await cancellation.CancelAsync();
        }
        finally
        {
            runs.OpenAll();
        }

        Exception? thrown = await Record.ExceptionAsync(() => scan);

        runs.CountStarted(request => request.WorkingDirectory == new WorkingFolder.At(repositories[8]) || request.WorkingDirectory == new WorkingFolder.At(repositories[9])).ShouldBe(0, "the ninth and tenth repositories were still waiting for a slot");
        thrown.ShouldBeAssignableTo<OperationCanceledException>();
    }

    /// <summary>Ten repositories under the root, each listing one worktree whose folder is gone, so each gives one row and no read beyond its listing.</summary>
    private string[] TenRepositories()
    {
        string[] repositories = [.. TenNames.Select(name => Path.Combine(Root, $"Northwind.{name}"))];
        foreach (string repository in repositories)
        {
            folders.WithDirectory(Path.Combine(repository, ".git", "objects"));
            processes.Answer(request => IsListingOf(request, repository), new ProcessResult(0, Listing(repository, $"{repository}-gone"), string.Empty));
        }

        return repositories;
    }

    private static bool IsStatus(ProcessRequest request)
    {
        return request.Arguments[0] == "status";
    }

    private static bool IsListing(ProcessRequest request)
    {
        return request.Arguments[0] == "worktree";
    }

    private static bool IsListingOf(ProcessRequest request, string repository)
    {
        return request.Arguments[0] == "worktree" && request.WorkingDirectory == new WorkingFolder.At(repository);
    }

    /// <summary>Git's porcelain listing of a repository and its linked worktrees, each on its own branch.</summary>
    private static string Listing(string repository, params string[] worktrees)
    {
        IEnumerable<string> linked = worktrees.Select(worktree => $"worktree {worktree.Replace('\\', '/')}\nHEAD b\nbranch refs/heads/{Path.GetFileName(worktree)}\n");

        return string.Join("\n", [$"worktree {repository.Replace('\\', '/')}\nHEAD a\nbranch refs/heads/main\n", .. linked]);
    }

    private void AnswerCounts(string dirty, string unpushed)
    {
        processes.Answer(request => request.Arguments[0] == "status", new ProcessResult(0, dirty, string.Empty));
        processes.Answer(request => request.Arguments[0] == "rev-list", new ProcessResult(0, unpushed, string.Empty));
    }

    private void AnswerListingWithOneWorktree()
    {
        processes.Answer(request => request.Arguments.Contains("--porcelain") && request.Arguments[0] == "worktree", new ProcessResult(0, "worktree C:/Users/me/source/repos/Northwind.Orders\nHEAD a\nbranch refs/heads/main\n\nworktree C:/Users/me/source/repos/Northwind.Orders-fix-123\nHEAD b\nbranch refs/heads/fix-123\n", string.Empty));
    }

    public void Dispose()
    {
        catalogue.Dispose();
    }

    /// <summary>
    /// Records every git run as it starts and holds the matching ones open, per folder, until the test opens them, so a
    /// fact can count what has started at once without sleeping.
    /// </summary>
    private sealed class HeldRuns(Func<ProcessRequest, bool> held)
    {
        private readonly Lock gate = new();
        private readonly List<ProcessRequest> started = [];
        private readonly Dictionary<WorkingFolder, TaskCompletionSource> holds = [];
        private readonly List<Waiter> waiters = [];
        private bool allOpen;

        public Task WhileRunning(ProcessRequest request)
        {
            lock (gate)
            {
                started.Add(request);
                foreach (Waiter waiter in waiters.Where(waiter => started.Count(waiter.Matches) >= waiter.Count))
                {
                    waiter.Reached.TrySetResult();
                }

                return held(request) ? HoldOf(request.WorkingDirectory).Task : Task.CompletedTask;
            }
        }

        public int CountStarted(Func<ProcessRequest, bool> matches)
        {
            lock (gate)
            {
                return started.Count(matches);
            }
        }

        public Task WhenStarted(Func<ProcessRequest, bool> matches, int count)
        {
            lock (gate)
            {
                Waiter waiter = new(matches, count, new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously));
                waiters.Add(waiter);
                if (started.Count(matches) >= count)
                {
                    waiter.Reached.TrySetResult();
                }

                return waiter.Reached.Task;
            }
        }

        public void Open(string folder)
        {
            lock (gate)
            {
                HoldOf(new WorkingFolder.At(folder)).TrySetResult();
            }
        }

        public void OpenAll()
        {
            lock (gate)
            {
                allOpen = true;
                foreach (TaskCompletionSource hold in holds.Values)
                {
                    hold.TrySetResult();
                }
            }
        }

        private TaskCompletionSource HoldOf(WorkingFolder folder)
        {
            if (!holds.TryGetValue(folder, out TaskCompletionSource? hold))
            {
                hold = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                holds[folder] = hold;
            }

            if (allOpen)
            {
                hold.TrySetResult();
            }

            return hold;
        }

        private sealed record Waiter(Func<ProcessRequest, bool> Matches, int Count, TaskCompletionSource Reached);
    }
}