using System.Reflection;
using Botyo.Configuration;
using Botyo.Extensions;
using Botyo.Middleware;
using Microsoft.Extensions.Options;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureLogging();

builder.Services
    .AddSwagger()
    .AddCommonDependencies()
    .AddBoundConfiguration(builder.Configuration, builder.Environment.IsDevelopment())
    .AddHttpClients(builder.Configuration)
    .AddLogging(x => x.AddConsole())
    .AddControllers()
    .AddSerializationOptions();

var app = builder.Build();

if (app.IsConfigurationValid())
{
    app.UseMiddleware<ApiKeyAuthentication>()
        .UseSwagger()
        .UseSwaggerUI()
        .UseHttpsRedirection();
    app
        .ConfigureOrmamu()
        .DeploySchema(app.Services.GetRequiredService<IOptions<PersistenceConfig>>().Value)
        .MapControllers();

    Log.Information("Running Botyo version {Version}", Assembly.GetExecutingAssembly().GetName().Version);
    await app.RunAsync();
}
else
{
    Log.Error("Please make sure you have configured your webhook URL as an environment variable before " +
                      "starting the service. The service will terminate now.");
}