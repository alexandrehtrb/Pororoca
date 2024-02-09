using System.Net;
using System.Text;
using Pororoca.Domain.Feature.Entities.Pororoca.Repetition;
using static Pororoca.Domain.Features.Common.HttpStatusCodeFormatter;

namespace Pororoca.Domain.Features.RequestRepeater;

public static class HttpRepetitionReporter
{
    public static async Task WriteReportAsync(
        IEnumerable<PororocaHttpRepetitionResult> results,
        string destinationFilePath)
    {
        using var fs = File.OpenWrite(destinationFilePath);
        using StreamWriter sw = new(fs);
        await WriteReportAsync(results, sw);
    }

    internal static async Task WriteReportAsync(
        IEnumerable<PororocaHttpRepetitionResult> results,
        StreamWriter sw)
    {
        string[] allInputDataKeys = results.SelectMany(r => (r.InputLine ?? []).Select(x => x.Key))
                                           .Distinct().ToArray();

        var headerLine = MakeCsvHeaderLine(allInputDataKeys);
        await sw.WriteLineAsync(headerLine);

        for (int i = 0; i < results.Count(); i++)
        {
            var r = results.ElementAt(i);
            var dataLine = MakeCsvDataLine(allInputDataKeys, i, r);
            await sw.WriteLineAsync(dataLine);
        }
    }

    private static StringBuilder MakeCsvHeaderLine(string[] allInputDataKeys)
    {
        StringBuilder sbHeader = new();
        sbHeader.Append("\"iteration\",\"result\",\"startedAt\",\"durationInMs\",");
        foreach (string key in allInputDataKeys)
        {
            sbHeader.Append("\"" + key + "\",");
        }
        return sbHeader;
    }

    private static StringBuilder MakeCsvDataLine(string[] allInputDataKeys, int i, PororocaHttpRepetitionResult r)
    {
        string result = GetResultStatusText(r);
        string startedAt = GetStartedAtText(r);
        long durationInMs = GetDurationInMs(r);

        StringBuilder sb = new($"\"{i+1}\",\"{result}\",\"{startedAt}\",\"{durationInMs}\"");
        foreach (string key in allInputDataKeys)
        {
            string value = r.InputLine?.FirstOrDefault(v => v.Key == key)?.Value?.Replace("\"", "\"\"") ?? string.Empty;
            sb.Append($",\"{value}\"");
        }
        sb.Append(',');

        return sb;
    }

    private static string GetResultStatusText(PororocaHttpRepetitionResult result)
    {
        if (result.Response?.Successful == true)
        {
            return FormatHttpStatusCodeText((HttpStatusCode)result.Response!.StatusCode!);
        }
        else if (result.Response?.Exception is TaskCanceledException || result.Response?.Exception is OperationCanceledException)
        {
            return "cancelled";
        }
        else if (result.Response?.Exception is not null)
        {
            return "exception";
        }
        else
        {
            return result.ValidationErrorCode!;
        }
    }

    private static string GetStartedAtText(PororocaHttpRepetitionResult result) =>
        result.Response?.StartedAtUtc.TimeOfDay.ToString(@"hh':'mm':'ss") ?? string.Empty;

    private static long GetDurationInMs(PororocaHttpRepetitionResult result) =>
        ((long?) result.Response?.ElapsedTime.TotalMilliseconds) ?? 0L;
}