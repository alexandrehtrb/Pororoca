using System.Text.Json;
using Pororoca.Domain.Features.Entities.Pororoca;
using static Pororoca.Domain.Features.Common.JsonConfiguration;

namespace Pororoca.Domain.Features.ExportCollection;

public static class PororocaCollectionExporter
{
    public static void ExportAsPororocaCollection(Stream streamToWrite, PororocaCollection col) =>
        JsonSerializer.Serialize(streamToWrite, col, MainJsonCtxWithConverters.PororocaCollection);

    public static Task ExportAsPororocaCollectionAsync(Stream streamToWrite, PororocaCollection col) =>
        JsonSerializer.SerializeAsync(streamToWrite, col, MainJsonCtxWithConverters.PororocaCollection);
}