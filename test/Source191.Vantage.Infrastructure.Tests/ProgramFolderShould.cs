using Source191.Vantage.Adapters.Files;

namespace Source191.Vantage.Infrastructure.Tests;

/// <summary>Where Vantage's own files are read from, with real files and, where Windows allows one, a real symbolic link.</summary>
public sealed class ProgramFolderShould : IDisposable
{
    private readonly string folder = Path.Combine(Path.GetTempPath(), $"vantage-program-{Guid.NewGuid():N}");
    private readonly string baseDirectory;

    public ProgramFolderShould()
    {
        baseDirectory = Directory.CreateDirectory(Path.Combine(folder, "base")).FullName;
    }

    public void Dispose()
    {
        if (Directory.Exists(folder))
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void BeTheBaseDirectoryForAProgramStartedDirectly()
    {
        string executable = Path.Combine(folder, "vantage.exe");
        File.WriteAllText(executable, "not really a program");

        ProgramFolder.Of(baseDirectory, executable).ShouldBe(baseDirectory);
    }

    [Fact]
    public void BeTheBaseDirectoryWithoutAProcessPath()
    {
        ProgramFolder.Of(baseDirectory, null).ShouldBe(baseDirectory);
    }

    [Fact]
    public void BeTheBaseDirectoryWhenTheProcessPathIsGone()
    {
        ProgramFolder.Of(baseDirectory, Path.Combine(folder, "gone.exe")).ShouldBe(baseDirectory);
    }

    [Fact]
    public void BeTheTargetsFolderForAProgramStartedThroughALink()
    {
        string package = Directory.CreateDirectory(Path.Combine(folder, "Packages", "Source191.Vantage")).FullName;
        string executable = Path.Combine(package, "vantage.exe");
        File.WriteAllText(executable, "not really a program");
        string links = Directory.CreateDirectory(Path.Combine(folder, "Links")).FullName;
        string link = Path.Combine(links, "vantage.exe");
        try
        {
            File.CreateSymbolicLink(link, executable);
        }
        catch (IOException)
        {
            Assert.Skip("Windows refused a symbolic link: it needs Developer Mode or an administrator.");
        }
        catch (UnauthorizedAccessException)
        {
            Assert.Skip("Windows refused a symbolic link: it needs Developer Mode or an administrator.");
        }

        ProgramFolder.Of(links, link).ShouldBe(package);
    }
}