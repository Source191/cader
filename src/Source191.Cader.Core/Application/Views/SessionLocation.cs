using Source191.Cader.Domain;

namespace Source191.Cader.Application.Views;

/// <summary>
/// Where a session runs: in a folder outside every repository (named by the folder, or by the root's text when git
/// names a root the console cannot read as a folder), in a repository, named by and looked up at its main checkout, or in
/// a folder that no longer exists, named by the folder and never asked of git.
/// </summary>
public abstract record SessionLocation
{
    public sealed record Outside(string Folder) : SessionLocation;

    public sealed record InRepository(string Name, FolderPath Checkout) : SessionLocation;

    public sealed record FolderGone(string Folder) : SessionLocation;
}