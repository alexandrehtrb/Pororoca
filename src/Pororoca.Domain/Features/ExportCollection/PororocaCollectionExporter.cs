using System.Text.Json;
using Pororoca.Domain.Features.Entities.Pororoca;
using static Pororoca.Domain.Features.Common.JsonConfiguration;

namespace Pororoca.Domain.Features.ExportCollection;

public static class PororocaCollectionExporter
{
    public static string ExportAsPororocaCollection(PororocaCollection col) =>
        JsonSerializer.Serialize(col, MainJsonCtxWithConverters.PororocaCollection);
}