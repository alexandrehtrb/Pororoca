using Serilog;
using Pororoca.TestServer.Configurations;
using Microsoft.AspNetCore.HttpLogging;
using System.Text;
using Pororoca.TestServer.Endpoints;

namespace ExemploProxyReverso.Api;

public static class Program
{
    public static int Main(string[] args)
    {
        var configuration = LoadConfigs();
        SetupSerilog(configuration);

        try
        {
            Log.Information("Starting web host");
            BuildApp(args).Run();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static IConfiguration LoadConfigs() =>
        new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

    private static void SetupSerilog(IConfiguration configuration) =>
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ApplicationName", typeof(Program).Assembly.GetName().Name)
            .WriteTo.Console()
            .CreateLogger();

    private static WebApplication BuildApp(string[] args)
    {
        var webAppBuilder = WebApplication.CreateBuilder(args);

        webAppBuilder.Host.ConfigureHost();
        webAppBuilder.Services.ConfigureServices();

        var webApp = webAppBuilder.Build();
        webApp.ConfigureApp();

        return webApp;
    }

    private static void ConfigureServices(this IServiceCollection services)
    {
        services.ConfigureJsonOptions();
        services.AddSerilogLogger();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }

    private static void ConfigureHost(this ConfigureHostBuilder builder) =>
        builder.UseSerilog();

    private static IApplicationBuilder ConfigureApp(this WebApplication app) =>
        app.MapTestEndpoints()
           .UseSwagger()
           .UseSwaggerUI();
}