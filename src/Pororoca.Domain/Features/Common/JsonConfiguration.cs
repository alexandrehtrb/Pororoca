using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Pororoca.Domain.Features.Entities.Pororoca.Repetition;
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

    internal static readonly MinifyJsonSrcGenContext MinifyingJsonCtx = MakeMinifyJsonContext();

    internal static readonly PrettifyJsonSrcGenContext PrettifyJsonCtx = MakePrettifyJsonContext();

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

    private static MinifyJsonSrcGenContext MakeMinifyJsonContext()
    {
        JsonSerializerOptions options = new();
        options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.WriteIndented = false;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.AllowTrailingCommas = true;
        options.ReadCommentHandling = JsonCommentHandling.Skip;
        return new(options);
    }

    private static PrettifyJsonSrcGenContext MakePrettifyJsonContext()
    {
        JsonSerializerOptions options = new();
        options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.WriteIndented = true;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
        options.PropertyNamingPolicy = null;
        return new(options);
    }
}

[JsonSerializable(typeof(PororocaCollection))]
[JsonSerializable(typeof(PororocaEnvironment))]
[JsonSerializable(typeof(PororocaHttpRequest))]
[JsonSerializable(typeof(PororocaWebSocketConnection))]
[JsonSerializable(typeof(PororocaHttpRepetition))]
[JsonSerializable(typeof(PostmanCollectionV21))]
[JsonSerializable(typeof(PostmanEnvironment))]
[JsonSerializable(typeof(PostmanAuthType))]
[JsonSerializable(typeof(PostmanRequestBodyMode))]
[JsonSerializable(typeof(PostmanRequestBodyFormDataParamType))]
[JsonSerializable(typeof(PostmanRequestUrl))]
[JsonSerializable(typeof(PostmanAuthBasic))]
[JsonSerializable(typeof(PostmanAuthBearer))]
[JsonSerializable(typeof(PostmanAuthNtlm))]
[JsonSerializable(typeof(PostmanVariable[]))]
internal partial class PororocaJsonSrcGenContext : JsonSerializerContext
{
}

[JsonSerializable(typeof(JsonDocument))]
[JsonSerializable(typeof(JsonElement))]
[JsonSerializable(typeof(Dictionary<string, string>[]))]
internal partial class MinifyJsonSrcGenContext : JsonSerializerContext
{
}

[JsonSerializable(typeof(JsonDocument))]
internal partial class PrettifyJsonSrcGenContext : JsonSerializerContext
{
}
