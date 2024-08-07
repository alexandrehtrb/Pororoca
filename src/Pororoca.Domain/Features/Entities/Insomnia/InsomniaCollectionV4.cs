using System.Text.Json.Serialization;

namespace Pororoca.Domain.Features.Entities.Insomnia;

#nullable disable warnings

public enum InsomniaCollectionV4ResourceType
{
    Workspace,
    RequestGroup,
    Request,
    Environment,
    Websocket
}

public sealed class InsomniaCollectionV4
{
    [JsonPropertyName("_type")]
    public string Type { get; set; }

    [JsonPropertyName("__export_format")]
    public int ExportFormat { get; set; }

    [JsonPropertyName("__export_date")]
    public string ExportDate { get; set; }

    [JsonPropertyName("__export_source")]
    public string ExportSource { get; set; }

    [JsonPropertyName("resources")]
    public InsomniaCollectionV4Resource[] Resources { get; set; }
}

public abstract class InsomniaCollectionV4Resource
{
    internal const string TypeExport = "export";
    internal const string TypeWorkspace = "workspace";
    internal const string TypeRequestGroup = "request_group";
    internal const string TypeRequest = "request";
    internal const string TypeEnvironment = "environment";
    internal const string TypeWebSocket = "websocket_request";

    public InsomniaCollectionV4ResourceType? TypeAsEnum => Type switch
    {
        TypeWorkspace => InsomniaCollectionV4ResourceType.Workspace,
        TypeRequestGroup => InsomniaCollectionV4ResourceType.RequestGroup,
        TypeRequest => InsomniaCollectionV4ResourceType.Request,
        TypeEnvironment => InsomniaCollectionV4ResourceType.Environment,
        TypeWebSocket => InsomniaCollectionV4ResourceType.Websocket,
        _ => null
    };

    [JsonPropertyName("_type")]
    public string Type { get; set; }

    [JsonPropertyName("_id")]
    public string Id { get; set; }

    [JsonPropertyName("parentId")]
    public string? ParentId { get; set; }

    public long Modified { get; set; }

    public long Created { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }
}

public sealed class InsomniaCollectionV4Workspace : InsomniaCollectionV4Resource
{
}

public sealed class InsomniaCollectionV4Environment : InsomniaCollectionV4Resource
{
    public Dictionary<string, object> Data { get; set; }
}

public sealed class InsomniaCollectionV4RequestGroup : InsomniaCollectionV4Resource
{
}

public sealed class InsomniaCollectionV4Request : InsomniaCollectionV4Resource
{
    public string Url { get; set; }
    public string Method { get; set; }
    public InsomniaCollectionV4RequestBody? Body { get; set; }
    public InsomniaCollectionV4NameValueParam[]? Headers { get; set; }
    public InsomniaCollectionV4RequestAuth? Authentication { get; set; }
}

public sealed class InsomniaCollectionV4WebSocket : InsomniaCollectionV4Resource
{
    public string Url { get; set; }
    public InsomniaCollectionV4NameValueParam[]? Headers { get; set; }
    public InsomniaCollectionV4RequestAuth? Authentication { get; set; }
}

public sealed class InsomniaCollectionV4NameValueParam
{
    public string Name { get; set; }
    public string Value { get; set; }
    public string Description { get; set; }
    public bool Disabled { get; set; }
}

public sealed class InsomniaCollectionV4RequestBody
{
    public string MimeType { get; set; }
    public string? Text { get; set; }
    public string? FileName { get; set; }
    public InsomniaCollectionV4NameValueParam[]? Params { get; set; }
}

public sealed class InsomniaCollectionV4RequestAuth
{
    public string Type { get; set; } // basic, bearer
    public string? Username { get; set; } // Basic auth
    public string? Password { get; set; } // Basic auth
    public string? Token { get; set; } // Bearer auth
}

#nullable restore warnings