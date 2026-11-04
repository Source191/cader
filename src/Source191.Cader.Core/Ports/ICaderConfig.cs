namespace Source191.Cader.Ports;

/// <summary>User configuration (config.json): <see cref="CaderConfig.Missing"/> when there is no file or it cannot be read.</summary>
public interface ICaderConfig
{
    Task<CaderConfig> ReadConfigAsync();

    Task WriteConfigAsync(CaderConfig config);
}