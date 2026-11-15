using Source191.Cader.Application;
using Source191.Cader.Application.Actions;
using Source191.Cader.Application.Effects;
using Source191.Cader.Application.Views;
using Source191.Cader.Domain;
using Source191.Cader.Ports;
using Source191.Cader.TestDoubles;

namespace Source191.Cader.Core.Tests.Application.Actions;

public sealed class OpenFileInEditorHandlerShould
{
    private const string LogFile = @"C:\Users\me\AppData\Local\Cader\cader.log";

    private readonly FakeFolders folders = new FakeFolders().WithFile(LogFile, "a line");
    private readonly FakeDesktop desktop = new();
    private readonly FakeEditorLocator locator = new FakeEditorLocator().Install(EditorPreset.VsCode, @"C:\Code\Code.exe");

    [Fact]
    public async Task StartTheChosenEditorOnTheFileItself()
    {
        OpenFileInEditorHandler handler = HandlerFor(EditorChoice.VsCode);

        Outcome outcome = await handler.HandleAsync(new OpenFileInEditor(FolderPath.From(LogFile)), CancellationToken.None);

        desktop.Started.Single().FileName.ShouldBe(@"C:\Code\Code.exe");
        desktop.Started.Single().Arguments.ShouldBe([LogFile]);
        outcome.ShouldBe(new OpenInEditorOutcome.Opened("cader.log", EditorChoice.VsCode));
    }

    [Fact]
    public async Task PassTheFileToACustomEditorAsOneArgument()
    {
        EditorChoice custom = EditorChoice.Custom(@"C:\Tools\subl.exe", ["-n", "{folder}"]);
        folders.WithFile(@"C:\Tools\subl.exe");

        Outcome outcome = await HandlerFor(custom).HandleAsync(new OpenFileInEditor(FolderPath.From(LogFile)), CancellationToken.None);

        desktop.Started.Single().Arguments.ShouldBe(["-n", LogFile]);
        outcome.ShouldBe(new OpenInEditorOutcome.Opened("cader.log", custom));
    }

    [Fact]
    public async Task StartNothingWhenTheFileDoesNotExistYet()
    {
        Outcome outcome = await HandlerFor(EditorChoice.VsCode).HandleAsync(new OpenFileInEditor(FolderPath.From(@"C:\Users\me\AppData\Local\Cader\config.json")), CancellationToken.None);

        desktop.Started.ShouldBeEmpty();
        outcome.ShouldBe(new OpenInEditorOutcome.FileMissing("config.json"));
    }

    [Fact]
    public async Task ReportAnEditorThatIsNotInstalled()
    {
        EditorChoice visualStudio = EditorChoice.Of(EditorPreset.VisualStudio);

        Outcome outcome = await HandlerFor(visualStudio).HandleAsync(new OpenFileInEditor(FolderPath.From(LogFile)), CancellationToken.None);

        desktop.Started.ShouldBeEmpty();
        outcome.ShouldBe(new OpenInEditorOutcome.EditorNotFound(visualStudio));
    }

    [Fact]
    public async Task ReportAnEditorThatWouldNotStart()
    {
        desktop.StartFailure = new ProgramNotStartedException("Access is denied.");

        Outcome outcome = await HandlerFor(EditorChoice.VsCode).HandleAsync(new OpenFileInEditor(FolderPath.From(LogFile)), CancellationToken.None);

        outcome.ShouldBe(new OpenInEditorOutcome.EditorNotStarted(EditorChoice.VsCode, "Access is denied."));
    }

    private OpenFileInEditorHandler HandlerFor(EditorChoice editor) => new(new EditorLauncher(locator, desktop, folders, CurrentWith(editor)), folders);

    private static CurrentSettings CurrentWith(EditorChoice editor)
    {
        return TestSettings.Current(Settings.Resolve([FolderPath.From(@"C:\Users\me\source\repos")], new WorktreeRootChoice.BesideFirstRoot(), editor, WorktreeBase.RemoteDefault, TestSettings.Home, checkForUpdates: Settings.ChecksForUpdatesWhenUnset, defaultPlace: Settings.PlaceWhenUnset, diagnosticLog: Settings.DiagnosticLogWhenUnset, beforeBranching: Settings.BeforeBranchingWhenUnset));
    }
}