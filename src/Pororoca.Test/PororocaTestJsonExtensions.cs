using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Pororoca.Domain.Features.Entities.Pororoca.Http;

namespace Pororoca.Test;

public static class PororocaTestJsonExtensions
{
    public static readonly JsonSerializerOptions MinifyJsonOptions = SetupMinifyJsonOptions();

    public static T? GetJsonBodyAs<T>(this PororocaHttpResponse res) =>
        res.GetJsonBodyAs<T>(MinifyJsonOptions);

    public static T? GetJsonBodyAs<T>(this PororocaHttpResponse res, JsonSerializerOptions jsonOptions) =>
        JsonSerializer.Deserialize<T>(res.GetBodyAsBinary(), jsonOptions);

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