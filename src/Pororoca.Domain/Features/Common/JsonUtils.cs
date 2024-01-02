using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pororoca.Domain.Features.Common;

public static class JsonUtils
{
    private static readonly JsonSerializerOptions PrettifyJsonOptions = SetupPrettifyJsonOptions();

    public static bool IsValidJson(string txt, JsonReaderOptions options = default)
    {
        byte[] utf8bytes = Encoding.UTF8.GetBytes(txt);
        Utf8JsonReader reader = new(utf8bytes, options);
        try
        {
            reader.Read();
        }
        catch
        {
            return false;
        }
        try
        {
            reader.TrySkip();
        }
        catch
        {
            return false;
        }
        return true;
    }

    public static string PrettifyJson(string json)
    {
        using var jDoc = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(jDoc, PrettifyJsonOptions);
    }

    public static string PrettySerializeJson(object? objToSerialize) =>
        JsonSerializer.Serialize(objToSerialize, options: PrettifyJsonOptions);

    private static JsonSerializerOptions SetupPrettifyJsonOptions()
    {
        JsonSerializerOptions options = new();
        options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.WriteIndented = true;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
        options.PropertyNamingPolicy = null;
        return options;
    }
}