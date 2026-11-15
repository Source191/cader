using Microsoft.Extensions.Time.Testing;
using Source191.Cader.Application.Diagnostics;
using Source191.Cader.Application.Launch;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application.Diagnostics;

/// <summary>What the new-session wizard tells the diagnostic log: the repositories it offered, and its first filter change once.</summary>
public sealed class WizardFilterWatchShould
{
    private const string Root = @"C:\Users\me\source\repos";
    private static readonly string Billing = Path.Combine(Root, "Northwind.Billing");

    private readonly FakeFolders folders = new FakeFolders()
        .WithDirectory(Path.Combine(Root, "Northwind.Orders", ".git"))
        .WithDirectory(Path.Combine(Billing, ".git"))
        .WithDirectory(Path.Combine(Root, "Northwind.Stock", ".git"));

    private readonly FakeTimeProvider clock = new(Fixtures.Noon);
    private readonly DiagnosticLog log;

    public WizardFilterWatchShould()
    {
        log = new DiagnosticLog(clock, []);
    }

    private IEnumerable<DiagnosticEvent> Recorded => log.Snapshot().Select(timed => timed.Event);

    private WizardState Opened(WizardOrigin origin) => WizardState.Open(new RepositoryCatalogue(folders, TestSettings.Current(Root)), [], folders, TestSettings.For(Root), origin);

    [Fact]
    public void RecordTheRepositoriesOfferedOnOpening()
    {
        WizardFilterWatch.Open(Opened(new WizardOrigin.Anywhere()), log, clock);

        Recorded.ShouldBe([new WizardOpened(3)]);
    }

    [Fact]
    public void RecordTheFirstFilterChangeOnce()
    {
        WizardFilterWatch watch = WizardFilterWatch.Open(Opened(new WizardOrigin.Anywhere()), log, clock);
        watch.ListFilled();
        clock.Advance(TimeSpan.FromMilliseconds(12));

        watch.FilterChanged(" ");
        watch.FilterChanged(" o");

        Recorded.ShouldBe([new WizardOpened(3), new WizardFilterStarted(TimeSpan.FromMilliseconds(12), CharacterKind.Whitespace, 1, ListFilled: true)]);
    }

    [Fact]
    public void SayWhenTheFirstChangeCameBeforeTheListWasFilled()
    {
        WizardFilterWatch watch = WizardFilterWatch.Open(Opened(new WizardOrigin.Anywhere()), log, clock);
        clock.Advance(TimeSpan.FromMilliseconds(1800));

        watch.FilterChanged("or");

        Recorded.Last().ShouldBe(new WizardFilterStarted(TimeSpan.FromMilliseconds(1800), CharacterKind.LetterOrDigit, 2, ListFilled: false));
    }

    [Fact]
    public void WaitForAFilterThatHoldsSomething()
    {
        WizardFilterWatch watch = WizardFilterWatch.Open(Opened(new WizardOrigin.Anywhere()), log, clock);

        watch.FilterChanged(string.Empty);
        Recorded.ShouldBe([new WizardOpened(3)], "an empty filter has no first character to record");

        watch.FilterChanged("​");
        Recorded.Last().ShouldBeOfType<WizardFilterStarted>().FirstCharacter.ShouldBe(CharacterKind.Format);
    }

    [Fact]
    public void RecordNothingForAFixedFolder()
    {
        WizardFilterWatch watch = WizardFilterWatch.Open(Opened(WizardOrigin.In(FolderPath.From(Billing))), log, clock);
        watch.FilterChanged("orders");

        Recorded.ShouldBeEmpty();
    }
}