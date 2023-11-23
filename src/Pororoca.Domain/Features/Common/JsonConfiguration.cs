using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.Entities.Postman;

namespace Pororoca.Domain.Features.Common;

internal static class JsonConfiguration
{
    internal static readonly PororocaJsonSrcGenContext MainJsonCtxWithConverters =
        MakePororocaJsonContext(true);
    
    internal static readonly PororocaJsonSrcGenContext MainJsonCtx =
        MakePororocaJsonContext(false);

    internal static readonly JsonSerializerOptions ViewJsonResponseOptions = SetupViewJsonResponseOptions();

    internal static readonly JsonSerializerOptions MinifyingOptions = SetupMinifyingOptions();

    private static PororocaJsonSrcGenContext MakePororocaJsonContext(bool includeCustomConverters)
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.WriteIndented = true;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

        if (includeCustomConverters)
        {
            options.Converters.Add(new PororocaRequestJsonConverter());
        }

        return new(options);
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

[JsonSerializable(typeof(PororocaCollection))]
[JsonSerializable(typeof(PororocaEnvironment))]
[JsonSerializable(typeof(PororocaHttpRequest))]
[JsonSerializable(typeof(PororocaWebSocketConnection))]
[JsonSerializable(typeof(PostmanCollectionV21))]
[JsonSerializable(typeof(PostmanEnvironment))]
internal partial class PororocaJsonSrcGenContext : JsonSerializerContext
{
}