namespace Pororoca.TestServer.Configurations;

public static class SerilogConfiguration
{
    public static IServiceCollection AddSerilogLogger(this IServiceCollection services) =>
        services.AddSingleton(Serilog.Log.Logger);
}