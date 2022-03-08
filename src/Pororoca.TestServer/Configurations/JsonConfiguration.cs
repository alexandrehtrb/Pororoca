using static Pororoca.Domain.Features.Common.JsonConfiguration;

namespace Pororoca.TestServer.Configurations
{
    public static class JsonConfiguration
    {
        public static IMvcBuilder AddDefaultJsonOptions(this IMvcBuilder mvcBuilder) =>
            mvcBuilder.AddJsonOptions(o => SetupExporterImporterJsonOptions(o.JsonSerializerOptions));
    }
}