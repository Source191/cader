using Source191.Cader.Ports;

namespace Source191.Cader.Application;

/// <summary>Which changes in a watched folder start a refresh, decided from the changed name alone.</summary>
public static class ChangeFilter
{
    public static bool Counts(WatchKind watch, FolderChange change) => false;
}