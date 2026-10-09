using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Botyo.Configuration;
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
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSwagger()
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

        public IServiceCollection AddCommonDependencies()
            => services
                .AddScoped<NotificationService>()
                .AddScoped<PersistenceService>()
                .AddScoped<IDispatchService, DiscordDispatchService>()
                .AddSingleton<WorkerService>()
                .AddHostedService<WorkerService>(x=>x.GetRequiredService<WorkerService>());

        public IServiceCollection AddHttpClients(IConfiguration configuration)
        {
            services
                .AddHttpClient("DiscordClient", x =>
                {
                    x.BaseAddress = new Uri(configuration.GetValue<string>("Discord:WebHook")!);
                    x.Timeout = TimeSpan.FromSeconds(300);
                });

            return services;
        }

        public IServiceCollection AddBoundConfiguration(IConfiguration configuration, bool isDevelopment)
        {
            services
                .Configure<ServerConfig>(configuration)
                .Configure<PersistenceConfig>(x=>
                {
                    x.ConnectionString = 
                        $"Data Source=\"{Path.Combine(
                            isDevelopment ? AppContext.BaseDirectory : "/data",
                            "persistence.db")}\"";
                });

            return services;
        }
    }

    public static void AddSerializationOptions(this IMvcBuilder builder)
    => builder.AddJsonOptions(x =>
        {
            x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        });

    extension(WebApplication app)
    {
        public WebApplication ConfigureOrmamu()
        {
            OrmamuConfig.Apply(new OrmamuOptions()
            {
                Dialect = SqlDialect.Sqlite
            });
            return app;
        }

        public WebApplication DeploySchema(PersistenceConfig config)
        {
            Log.Information("Connecting to database {0}", config.ConnectionString);
        
            SQLitePCL.Batteries.Init();              
            UpgradeEngine engine =
                DeployChanges.To
                    .SqliteDatabase(config.ConnectionString)
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

        public bool IsConfigurationValid()
            => !string.IsNullOrEmpty(app.Configuration.GetValue<string>("Discord:WebHook"));
    }

    public static void ConfigureLogging(this WebApplicationBuilder builder)
        => builder.Host.UseSerilog((_, loggerConfiguration) => loggerConfiguration
            .WriteTo.Console()
            .ReadFrom.Configuration(builder.Configuration));
}