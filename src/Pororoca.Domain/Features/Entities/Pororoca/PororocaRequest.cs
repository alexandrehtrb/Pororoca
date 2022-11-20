using System.Text.Json.Serialization;

namespace Pororoca.Domain.Features.Entities.Pororoca;

public abstract class PororocaRequest : PororocaCollectionItem
{
    [JsonPropertyOrder(-3)]
    public PororocaRequestType RequestType { get; init; }

    protected PororocaRequest(PororocaRequestType reqType, Guid id, string name) : base(id, name) =>
        RequestType = reqType;
}