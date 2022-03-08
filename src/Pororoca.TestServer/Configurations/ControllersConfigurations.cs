using Pororoca.TestServer.Configurations.Filters;

namespace Pororoca.TestServer.Configurations
{
    public static class ControllersConfigurations
    {
        public static IMvcBuilder AddCustomControllers(this IServiceCollection services) =>
            services.AddControllers(c =>
                {
                    c.Filters.Add<LogActionFilter>();
                });
    }
}