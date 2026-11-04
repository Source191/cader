using Source191.Cader.Domain;

namespace Source191.Cader.Ports;

/// <summary>The released version of Cader that last ran, so the first start after an update says what changed, once.</summary>
public interface ILastRunVersion
{
    Task<LastRun> ReadLastRunAsync();

    Task WriteLastRunAsync(ReleaseVersion version);
}