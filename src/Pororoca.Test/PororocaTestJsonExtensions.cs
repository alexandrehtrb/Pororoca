using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.TranslateRequest.WebSockets.ClientMessage;

namespace Pororoca.Test;

public static class PororocaTestJsonExtensions
{
    private static readonly JsonSerializerOptions minifyOptions = SetupMinifyJsonOptions();

    public static T? GetJsonBodyAs<T>(this PororocaHttpResponse res) =>
        res.GetJsonBodyAs<T>(minifyOptions);

    public static T? GetJsonBodyAs<T>(this PororocaHttpResponse res, JsonSerializerOptions jsonOptions) =>
        JsonSerializer.Deserialize<T>(res.GetBodyAsBinary(), jsonOptions);

    public static T? ReadAsJson<T>(this PororocaWebSocketClientMessageToSend wsCliMsg) =>
        wsCliMsg.ReadAsJson<T>(minifyOptions);

    public static T? ReadAsJson<T>(this PororocaWebSocketClientMessageToSend wsCliMsg, JsonSerializerOptions jsonOptions) =>
        JsonSerializer.Deserialize<T>(wsCliMsg.Text!, jsonOptions);

    public static T? ReadAsJson<T>(this PororocaWebSocketServerMessage srvMsg) =>
        srvMsg.ReadAsJson<T>(minifyOptions);

    public static T? ReadAsJson<T>(this PororocaWebSocketServerMessage srvMsg, JsonSerializerOptions jsonOptions) =>
        JsonSerializer.Deserialize<T>(srvMsg.Text!, jsonOptions);

    private static JsonSerializerOptions SetupMinifyJsonOptions()
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
}