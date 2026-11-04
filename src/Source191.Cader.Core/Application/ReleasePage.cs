using Source191.Cader.Domain;

namespace Source191.Cader.Application;

/// <summary>
/// The one home of a release's GitHub page, built from the version alone: no text from the release notes ever reaches
/// a URL.
/// </summary>
public static class ReleasePage
{
    public static Uri For(ReleaseVersion version) => new($"https://github.com/Source191/cader/releases/tag/v{version}");
}