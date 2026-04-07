using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pororoca.Test.Tests;

[JsonSerializable(typeof(Dictionary<string, int>))]
[JsonSerializable(typeof(TestServerWebSocketMessage))]
[JsonSourceGenerationOptions(
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    UseStringEnumConverter = true,
    AllowTrailingCommas = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    GenerationMode = JsonSourceGenerationMode.Default)]
public partial class TestsJsonSrcGenContext : JsonSerializerContext
{
}
