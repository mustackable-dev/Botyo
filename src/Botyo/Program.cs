using Botyo.Extensions;
using Botyo.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureLogging();

builder.Services
    .AddSwagger()
    .AddCommonDependencies()
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
        .DeploySchema()
        .MapControllers();

    await app.RunAsync();
}
else
{
    Log.Error("Please make sure you have configured your webhook URL as an environment variable before " +
                      "starting the service. The service will terminate now.");
}