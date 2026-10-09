namespace Botyo.Configuration;

public record ServerConfig
{
    public string ApiKey { get; set; } = "";
    public DiscordConfig Discord { get; set; } = null!;
}