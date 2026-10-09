namespace Botyo.Configuration;

public record PersistenceConfig
{
    public string ConnectionString { get; set; } = "";
}