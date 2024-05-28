using System.Text.Json.Serialization;

namespace Pororoca.Domain.Features.Entities.Pororoca;

public enum PororocaRequestType
{
    Http,
    Websocket,
    HttpRepetition
}

public abstract record PororocaRequest
(
    [property: JsonPropertyOrder(-3)] PororocaRequestType RequestType,
    [property: JsonPropertyOrder(-2)] string Name
)
{
    public abstract PororocaRequest CopyAbstract();
}