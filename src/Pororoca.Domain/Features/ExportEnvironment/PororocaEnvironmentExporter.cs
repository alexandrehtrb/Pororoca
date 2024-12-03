using System.Text.Json;
using Pororoca.Domain.Features.Entities.Pororoca;
using static Pororoca.Domain.Features.Common.JsonConfiguration;

namespace Pororoca.Domain.Features.ExportEnvironment;

public static class PororocaEnvironmentExporter
{
    public static byte[] ExportAsPororocaEnvironment(PororocaEnvironment env) =>
        JsonSerializer.SerializeToUtf8Bytes(env, MainJsonCtx.PororocaEnvironment);
}