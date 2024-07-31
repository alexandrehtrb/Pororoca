using System.Text.Json;
using Pororoca.Domain.Features.Entities.Pororoca;
using static Pororoca.Domain.Features.Common.JsonConfiguration;

namespace Pororoca.Domain.Features.ExportEnvironment;

public static class PororocaEnvironmentExporter
{
    public static string ExportAsPororocaEnvironment(PororocaEnvironment env) =>
        JsonSerializer.Serialize(env, MainJsonCtx.PororocaEnvironment);
}