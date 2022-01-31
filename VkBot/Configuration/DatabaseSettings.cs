namespace VkBot.Configuration;

public class DatabaseSettings
{
    public DatabaseSettings()
    {
    }

    public string ConnectionString { get; init; } = null!;
}