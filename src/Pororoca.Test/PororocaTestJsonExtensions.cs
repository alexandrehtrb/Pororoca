using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Pororoca.Domain.Features.Entities.Pororoca.Http;

namespace Pororoca.Test;

public static class PororocaTestJsonExtensions
{
    public static T? GetJsonBodyAs<T>(this PororocaHttpResponse res, JsonTypeInfo<T> jsonTypeInfo) =>
        JsonSerializer.Deserialize(res.GetBodyAsBinary(), jsonTypeInfo);
}