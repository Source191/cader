namespace Source191.Cader.Ports;

/// <summary>The last update check, so Cader asks at most once a day; only a check that was made is written.</summary>
public interface IUpdateCheckRecords
{
    Task<UpdateCheckRecord> ReadUpdateCheckAsync();

    Task WriteUpdateCheckAsync(UpdateCheckRecord.Checked record);
}