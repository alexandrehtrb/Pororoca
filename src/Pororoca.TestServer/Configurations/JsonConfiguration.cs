using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pororoca.TestServer.Configurations
{
    public static class JsonConfiguration
    {
        public static IMvcBuilder AddDefaultJsonOptions(this IMvcBuilder mvcBuilder) =>
            mvcBuilder.AddJsonOptions(o => SetupExporterImporterJsonOptions(o.JsonSerializerOptions));

        private static JsonSerializerOptions SetupExporterImporterJsonOptions(JsonSerializerOptions options)
        {
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            options.WriteIndented = true;
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            return options;
        }
    }
}