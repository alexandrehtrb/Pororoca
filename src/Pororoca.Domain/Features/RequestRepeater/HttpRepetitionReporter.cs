using System.Net;
using System.Text;
using Pororoca.Domain.Features.Entities.Pororoca.Repetition;
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
        StringBuilder sbCsv = new();
        // Microsoft Excel reads the first CSV line "sep=,"
        // and understands comma as separator for values,
        // loading the CSV as a table.
        sbCsv.AppendLine("sep=,");
        sbCsv.Append(MakeCsvHeaderLine(allInputDataKeys));
        sbCsv.AppendLine();

        int count = results.Count();
        for (int i = 0; i < count; i++)
        {
            var r = results.ElementAt(i);
            sbCsv.Append(MakeCsvDataLine(allInputDataKeys, i, r));
            sbCsv.AppendLine();
        }

        await sw.WriteAsync(sbCsv);
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

        StringBuilder sb = new($"\"{i + 1}\",\"{result}\",\"{startedAt}\",\"{durationInMs}\"");
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
        ((long?)result.Response?.ElapsedTime.TotalMilliseconds) ?? 0L;
}