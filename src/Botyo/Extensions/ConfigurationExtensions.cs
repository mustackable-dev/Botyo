using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Botyo.Contracts;
using DbUp;
using DbUp.Engine;
using Botyo.Services;
using Microsoft.OpenApi;
using Ormamu;
using Serilog;

namespace Botyo.Extensions;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(x =>
        {
            x.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Botyo",
                Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "",
            });
            x.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                Name = "ApiKey",
                In = ParameterLocation.Header
            });

            x.AddSecurityRequirement(y => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("ApiKey", y),
                    []
                }
            });
        });
        return services;
    }

    public static bool IsConfigurationValid(this WebApplication app)
        => !string.IsNullOrEmpty(app.Configuration.GetValue<string>("Discord:WebHook"));

    public static IServiceCollection AddCommonDependencies(this IServiceCollection services)
        => services
            .AddTransient<NotificationService>()
            .AddTransient<PersistenceService>()
            .AddTransient<IDispatchService, DiscordDispatchService>()
            .AddSingleton<WorkerService>()
            .AddHostedService<WorkerService>(x=>x.GetRequiredService<WorkerService>());

    public static void AddSerializationOptions(this IMvcBuilder builder)
    => builder.AddJsonOptions(x =>
        {
            x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        });

    public static WebApplication ConfigureOrmamu(this WebApplication app)
    {
        OrmamuConfig.Apply(new OrmamuOptions()
        {
            Dialect = SqlDialect.Sqlite
        });
        return app;
    }

    public static WebApplication DeploySchema(this WebApplication app)
    {
        PersistenceService.ConnectionString = $"Data Source=\"{Path.Combine(
            app.Environment.IsDevelopment() ? AppContext.BaseDirectory : "/data",
            "persistence.db")}\"";
        
        Log.Information("Connecting to database {0}", PersistenceService.ConnectionString);
        
        SQLitePCL.Batteries.Init();              
        UpgradeEngine engine =
            DeployChanges.To
                .SqliteDatabase(PersistenceService.ConnectionString)
                .JournalToSqliteTable("SchemaVersions")
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(),x => x.EndsWith(".sql"))
                .LogToConsole()
                .Build();

        DatabaseUpgradeResult result = engine.PerformUpgrade();

        if (!result.Successful)
        {
            Log.Fatal(result.Error, "Error occured when applying script " +
                                    "{ScriptName} on persistence layer with content:\n\n" +
                                    "{Contents}", result.ErrorScript.Name, result.ErrorScript.Contents);
            throw result.Error;
        }

        Log.Information("Schema deploy successful!");
        
        return app;
    }
    public static IServiceCollection AddHttpClients(this IServiceCollection services , IConfiguration configuration)
    {
        services
            .AddHttpClient("DiscordClient", x =>
            {
                x.BaseAddress = new Uri(configuration.GetValue<string>("Discord:WebHook")!);
                x.Timeout = TimeSpan.FromSeconds(300);
            });

        return services;
    }

    public static void ConfigureLogging(this WebApplicationBuilder builder)
        => builder.Host.UseSerilog((_, loggerConfiguration) => loggerConfiguration
            .WriteTo.Console()
            .ReadFrom.Configuration(builder.Configuration));
}