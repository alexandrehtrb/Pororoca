using System.Text.Json;
using System.Text.Json.Serialization;
using Pororoca.Domain.Features.Entities.Insomnia;
using static Pororoca.Domain.Features.Entities.Insomnia.InsomniaCollectionV4Resource;
using static Pororoca.Domain.Features.Common.JsonConfiguration;

namespace Pororoca.Domain.Features.Common;

// Based on:
// https://blog.maartenballiauw.be/post/2020/01/29/deserializing-json-into-polymorphic-classes-with-systemtextjson.html
public sealed class InsomniaResourceJsonConverter : JsonConverter<InsomniaCollectionV4Resource?>
{
    public override bool CanConvert(Type typeToConvert) =>
        typeof(InsomniaCollectionV4Resource).IsAssignableFrom(typeToConvert);

    public override InsomniaCollectionV4Resource? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Check for null values
        if (reader.TokenType == JsonTokenType.Null)
            throw new JsonException("A request cannot be null.");

        // Copy the current state from reader (it's a struct)
        var readerAtStart = reader;

        using var jsonDocument = JsonDocument.ParseValue(ref reader);
        var jsonObject = jsonDocument.RootElement;

        if (!jsonObject.TryGetProperty("_type", out var resourceTypeJsonElement))
        {
            return null;
        }

        string? resourceTypeStr = resourceTypeJsonElement.GetString();
        if (resourceTypeStr is null)
        {
            return null;
        }
        
        // The custom converters cannot be used below, otherwise, a recursive call will happen
        // and a StackOverflowExcpetion will arise
        return resourceTypeStr switch
        {
            TypeWorkspace => JsonSerializer.Deserialize(ref readerAtStart, MainJsonCtx.InsomniaCollectionV4Workspace)!,
            TypeRequestGroup => JsonSerializer.Deserialize(ref readerAtStart, MainJsonCtx.InsomniaCollectionV4RequestGroup)!,
            TypeRequest => JsonSerializer.Deserialize(ref readerAtStart, MainJsonCtx.InsomniaCollectionV4Request)!,
            TypeEnvironment => JsonSerializer.Deserialize(ref readerAtStart, MainJsonCtx.InsomniaCollectionV4Environment)!,
            TypeWebSocket => JsonSerializer.Deserialize(ref readerAtStart, MainJsonCtx.InsomniaCollectionV4WebSocket)!,
            _ => null
        };
    }

    public override void Write(Utf8JsonWriter writer, InsomniaCollectionV4Resource? resource, JsonSerializerOptions options) =>
        throw new NotImplementedException(); // no need because we only import Insomnia collections
}