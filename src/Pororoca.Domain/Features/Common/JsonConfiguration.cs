using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Pororoca.Domain.Features.ImportCollection;

namespace Pororoca.Domain.Features.Common;

internal static class JsonConfiguration
{
    internal static readonly JsonSerializerOptions ExporterImporterJsonOptions =
        SetupCustomConvertersInJsonOptions(SetupExporterImporterJsonOptions(new JsonSerializerOptions()));

    internal static readonly JsonSerializerOptions ExporterImporterWithoutCustomConvertersJsonOptions =
        SetupExporterImporterJsonOptions(new JsonSerializerOptions());

    internal static readonly JsonSerializerOptions ViewJsonResponseOptions = SetupViewJsonResponseOptions();

    internal static readonly JsonSerializerOptions MinifyingOptions = SetupMinifyingOptions();

    private static JsonSerializerOptions SetupExporterImporterJsonOptions(JsonSerializerOptions options)
    {
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.WriteIndented = true;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        return options;
    }

    private static JsonSerializerOptions SetupCustomConvertersInJsonOptions(JsonSerializerOptions options)
    {
        options.Converters.Add(new PororocaRequestJsonConverter());
        return options;
    }

    private static JsonSerializerOptions SetupViewJsonResponseOptions()
    {
        JsonSerializerOptions options = new();
        options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.WriteIndented = true;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
        options.PropertyNamingPolicy = null;
        return options;
    }

    private static JsonSerializerOptions SetupMinifyingOptions()
    {
        JsonSerializerOptions options = new();
        options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.WriteIndented = false;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        return options;
    }
}