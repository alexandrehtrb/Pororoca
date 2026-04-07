using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Pororoca.Domain.Features.Entities.GitHub;
using Pororoca.Domain.Features.Entities.Insomnia;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Entities.Pororoca.Repetition;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.Entities.Postman;

namespace Pororoca.Domain.Features.Common;

internal static class JsonConfiguration
{
    internal static readonly PororocaJsonSrcGenContext MainJsonCtxWithConverters =
        PororocaJsonSrcGenContext.WithConverters;

    internal static readonly PororocaJsonSrcGenContext MainJsonCtx =
        PororocaJsonSrcGenContext.WithoutConverters;

    internal static readonly MinifyJsonSrcGenContext MinifyingJsonCtx = MakeMinifyJsonContext();

    internal static readonly PrettifyJsonSrcGenContext PrettifyJsonCtx = MakePrettifyJsonContext();

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
[JsonSerializable(typeof(InsomniaCollectionV4))]
[JsonSerializable(typeof(InsomniaCollectionV4Resource))]
[JsonSerializable(typeof(InsomniaCollectionV4Workspace))]
[JsonSerializable(typeof(InsomniaCollectionV4Environment))]
[JsonSerializable(typeof(InsomniaCollectionV4RequestGroup))]
[JsonSerializable(typeof(InsomniaCollectionV4Request))]
[JsonSerializable(typeof(InsomniaCollectionV4WebSocket))]
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    UseStringEnumConverter = true,
    AllowTrailingCommas = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    GenerationMode = JsonSourceGenerationMode.Default)]
internal partial class PororocaJsonSrcGenContext : JsonSerializerContext
{
    internal static readonly PororocaJsonSrcGenContext WithConverters;
    internal static readonly PororocaJsonSrcGenContext WithoutConverters;

    static PororocaJsonSrcGenContext()
    {
        WithConverters = new PororocaJsonSrcGenContext(CreateJsonSerializerOptions(includeCustomConverters: true));
        WithoutConverters = new PororocaJsonSrcGenContext(CreateJsonSerializerOptions(includeCustomConverters: false));
    }

    private static JsonSerializerOptions CreateJsonSerializerOptions(bool includeCustomConverters)
    {
        var options = new JsonSerializerOptions(Default.GeneratedSerializerOptions!)
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        if (includeCustomConverters)
        {
            options.Converters.Add(new PororocaRequestJsonConverter());
            options.Converters.Add(new InsomniaResourceJsonConverter());
        }

        return options;
    }
}

[JsonSerializable(typeof(JsonDocument))]
[JsonSerializable(typeof(JsonElement))]
[JsonSerializable(typeof(Dictionary<string, string>[]))]
[JsonSerializable(typeof(GitHubGetReleaseResponse))]
internal partial class MinifyJsonSrcGenContext : JsonSerializerContext
{
}

[JsonSerializable(typeof(JsonDocument))]
[JsonSerializable(typeof(JsonNode))]
[JsonSerializable(typeof(JsonObject))]
[JsonSerializable(typeof(JsonArray))]
[JsonSerializable(typeof(JsonValue))]
internal partial class PrettifyJsonSrcGenContext : JsonSerializerContext
{
}