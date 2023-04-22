#nullable disable warnings

using System.Text.Json;
using static Pororoca.Domain.Features.Common.JsonConfiguration;

namespace Pororoca.Domain.Features.Entities.Postman;

internal enum PostmanAuthType
{
    // TODO: Rename enum values according to C# style convention,
    // but preserving JSON serialization and deserialization
    noauth,
    basic,
    oauth1,
    oauth2,
    bearer,
    digest,
    apikey,
    awsv4,
    hawk,
    ntlm
}

internal class PostmanAuth
{
    public PostmanAuthType Type { get; set; }

    public object? Basic { get; set; }

    public object? Bearer { get; set; }

    public (string basicAuthLogin, string basicAuthPwd) ReadBasicAuthValues()
    {
        static (string, string) ParseFromVariableArray(PostmanVariable[] arr) =>
            (arr.FirstOrDefault(p => p.Key == "username")?.Value ?? string.Empty,
             arr.FirstOrDefault(p => p.Key == "password")?.Value ?? string.Empty);

        if (Basic is JsonElement je)
        {
            if (je.ValueKind == JsonValueKind.Object)
            {
                var basic = je.Deserialize<PostmanAuthBasic>(options: ExporterImporterJsonOptions);
                return (basic.Username ?? string.Empty, basic.Password ?? string.Empty);
            }
            else if (je.ValueKind == JsonValueKind.Array)
            {
                var basic = je.Deserialize<PostmanVariable[]>();
                return ParseFromVariableArray(basic);
            }
        }
        else if (Basic is PostmanVariable[] arr)
        {
            return ParseFromVariableArray(arr);
        }

        return (string.Empty, string.Empty);
    }

    public string ReadBearerAuthValue()
    {
        static string ParseFromVariableArray(PostmanVariable[] arr) =>
            arr.FirstOrDefault(p => p.Key == "token")?.Value ?? string.Empty;

        if (Bearer is JsonElement je)
        {
            if (je.ValueKind == JsonValueKind.Object)
            {
                var bearer = je.Deserialize<PostmanAuthBearer>(options: ExporterImporterJsonOptions);
                return bearer.Token ?? string.Empty;
            }
            else if (je.ValueKind == JsonValueKind.Array)
            {
                var bearer = je.Deserialize<PostmanVariable[]>();
                return ParseFromVariableArray(bearer);
            }
        }
        else if (Bearer is PostmanVariable[] arr)
        {
            return ParseFromVariableArray(arr);
        }

        return string.Empty;
    }
}

internal class PostmanAuthBasic
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}

internal class PostmanAuthBearer
{
    public string? Token { get; set; }
}

#nullable enable warnings