namespace Pororoca.TestServer.Endpoints;

public static class WebApplicationExtensions
{
    public static IEndpointConventionBuilder MapConnect(this WebApplication app, string pattern, Delegate reqDelegate) =>
        app.MapMethods(pattern, new[] { HttpMethods.Connect }, reqDelegate);
}