using System.Net;
using System.Text;
using System.Text.Json;
using Pororoca.Domain.Features.Entities.Pororoca.Repetition;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.TranslateRequest;
using Xunit;
using static Pororoca.Domain.Tests.Features.Entities.Pororoca.Http.PororocaHttpResponseTests;
using static Pororoca.Domain.Features.RequestRepeater.HttpRepetitionReporter;

namespace Pororoca.Domain.Tests.Features.RequestRepeater;

public static class HttpRepetitionReporterTests
{
    [Fact]
    public static async Task Should_produce_report_with_various_results_and_without_input_lines_properly()
    {
        // GIVEN
        using MemoryStream ms = new();
        using StreamWriter sw = new(ms);
        var results = await MakeExampleResultsAsync();

        // WHEN
        await WriteReportAsync(results, sw);
        sw.Flush();
        string csv = Encoding.UTF8.GetString(ms.ToArray());

        // THEN
        StringBuilder sb = new();
        sb.AppendLine(@"""iteration"",""result"",""startedAt"",""durationInMs"",");
        sb.AppendLine(@"""1"",""TranslateRequest_InvalidUrl"","""",""0"",");
        sb.AppendLine(@"""2"",""200 OK"",""19:05:17"",""187"",");
        sb.AppendLine(@"""3"",""500 InternalServerError"",""19:05:17"",""187"",");
        sb.AppendLine(@"""4"",""cancelled"",""19:05:17"",""187"",");
        sb.AppendLine(@"""5"",""exception"",""19:05:17"",""187"",");
        Assert.Equal(sb.ToString(), csv);
    }

    [Fact]
    public static async Task Should_produce_report_with_homogeneous_input_line_variables_properly()
    {
        // GIVEN
        using MemoryStream ms = new();
        using StreamWriter sw = new(ms);
        var results = await MakeExampleResultsAsync();
        results[0] = results[0] with { InputLine = [new(true, "Z", "1", true), new(true, "Elemento", "Hidrogênio", true)] };
        results[1] = results[1] with { InputLine = [new(true, "Z", "7", true), new(true, "Elemento", "Nitrogênio", true)] };
        results[2] = results[2] with { InputLine = [new(true, "Z", "10", true), new(true, "Elemento", "\"Neônio\"", true)] };
        results[3] = results[3] with { InputLine = [new(true, "Z", "15", true), new(true, "Elemento", "Fósforo", true)] };
        results[4] = results[4] with { InputLine = [new(true, "Z", "80", true), new(true, "Elemento", "Mercúrio", true)] };

        // WHEN
        await WriteReportAsync(results, sw);
        sw.Flush();
        string csv = Encoding.UTF8.GetString(ms.ToArray());

        // THEN
        StringBuilder sb = new();
        sb.AppendLine(@"""iteration"",""result"",""startedAt"",""durationInMs"",""Z"",""Elemento"",");
        sb.AppendLine(@"""1"",""TranslateRequest_InvalidUrl"","""",""0"",""1"",""Hidrogênio"",");
        sb.AppendLine(@"""2"",""200 OK"",""19:05:17"",""187"",""7"",""Nitrogênio"",");
        sb.AppendLine(@"""3"",""500 InternalServerError"",""19:05:17"",""187"",""10"",""""""Neônio"""""",");
        sb.AppendLine(@"""4"",""cancelled"",""19:05:17"",""187"",""15"",""Fósforo"",");
        sb.AppendLine(@"""5"",""exception"",""19:05:17"",""187"",""80"",""Mercúrio"",");
        Assert.Equal(sb.ToString(), csv);
    }

    [Fact]
    public static async Task Should_produce_report_with_heterogeneous_input_line_variables_properly()
    {
        // GIVEN
        using MemoryStream ms = new();
        using StreamWriter sw = new(ms);
        var results = await MakeExampleResultsAsync();
        results[0] = results[0] with { InputLine = [] };
        results[1] = results[1] with { InputLine = [new(true, "Z", "7", true), new(true, "Elemento", "Nitrogênio", true)] };
        results[2] = results[2] with { InputLine = [new(true, "Elemento", "\"Neônio\"", true)] };
        results[3] = results[3] with { InputLine = [new(true, "Z", "15", true), new(true, "Elemento", "Fósforo", true)] };
        results[4] = results[4] with { InputLine = [new(true, "Elemento", "Mercúrio", true), new(true, "Z", "80", true)] };

        // WHEN
        await WriteReportAsync(results, sw);
        sw.Flush();
        string csv = Encoding.UTF8.GetString(ms.ToArray());

        // THEN
        StringBuilder sb = new();
        sb.AppendLine(@"""iteration"",""result"",""startedAt"",""durationInMs"",""Z"",""Elemento"",");
        sb.AppendLine(@"""1"",""TranslateRequest_InvalidUrl"","""",""0"","""","""",");
        sb.AppendLine(@"""2"",""200 OK"",""19:05:17"",""187"",""7"",""Nitrogênio"",");
        sb.AppendLine(@"""3"",""500 InternalServerError"",""19:05:17"",""187"","""",""""""Neônio"""""",");
        sb.AppendLine(@"""4"",""cancelled"",""19:05:17"",""187"",""15"",""Fósforo"",");
        sb.AppendLine(@"""5"",""exception"",""19:05:17"",""187"",""80"",""Mercúrio"",");
        Assert.Equal(sb.ToString(), csv);
    }

    private static async Task<PororocaHttpRepetitionResult[]> MakeExampleResultsAsync() =>
    [
        new(null, null, TranslateRequestErrors.InvalidUrl),
        new(null, await MakeExampleResponseAsync(HttpStatusCode.OK, null), null),
        new(null, await MakeExampleResponseAsync(HttpStatusCode.InternalServerError, null), null),
        new(null, await MakeExampleResponseAsync(null, new TaskCanceledException()), null),
        new(null, await MakeExampleResponseAsync(null, new JsonException()), null)
    ];

    internal static async Task<PororocaHttpResponse> MakeExampleResponseAsync(HttpStatusCode? statusCode, Exception? ex)
    {
        var startedAt = DateTimeOffset.Parse("2020-05-23T19:05:17.1581236");
        var elapsedTime = TimeSpan.FromMilliseconds(187);

        if (ex is not null)
        {
            return PororocaHttpResponse.Failed(null!, startedAt, elapsedTime, ex);
        }
        else
        {
            using var resMsg = CreateTestHttpResponseMessage("oi", "text/plain", null);
            resMsg.StatusCode = (HttpStatusCode)statusCode!;
            return await PororocaHttpResponse.SuccessfulAsync(null!, startedAt, elapsedTime, resMsg);
        }
    }
}