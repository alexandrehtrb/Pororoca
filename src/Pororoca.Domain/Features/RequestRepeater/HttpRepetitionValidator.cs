using System.Text.Json;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Entities.Pororoca.Repetition;
using Pororoca.Domain.Features.VariableResolution;
using static Pororoca.Domain.Features.Common.JsonConfiguration;

namespace Pororoca.Domain.Features.RequestRepeater;

public static class HttpRepetitionValidator
{
    public static async Task<(bool Valid, string? ErrorCode, PororocaVariable[][]? ResolvedInputData)> IsValidRepetitionAsync(
        IEnumerable<PororocaVariable> collectionEffectiveVariables,
        PororocaHttpRequest? baseReq,
        PororocaHttpRepetition rep,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rep.BaseRequestPath))
            return (false, TranslateRepetitionErrors.BaseHttpRequestNotSelected, null);

        if (baseReq is null)
            return (false, TranslateRepetitionErrors.BaseHttpRequestNotFound, null);

        if (rep.DelayInMs is not null && rep.DelayInMs < 0L)
            return (false, TranslateRepetitionErrors.DelayCantBeNegative, null);

        if (rep.MaxRatePerSecond is not null && rep.MaxRatePerSecond < 0L)
            return (false, TranslateRepetitionErrors.MaximumRateCantBeNegative, null);

        if (rep.RepetitionMode == PororocaRepetitionMode.Simple
         || rep.RepetitionMode == PororocaRepetitionMode.Random)
        {
            if (rep.NumberOfRepetitions < 1)
                return (false, TranslateRepetitionErrors.NumberOfRepetitionsMustBeAtLeast1, null);

            if (rep.MaxDop < 1)
                return (false, TranslateRepetitionErrors.MaxDopMustBeAtLeast1, null);
        }

        if (rep.RepetitionMode == PororocaRepetitionMode.Sequential
         || rep.RepetitionMode == PororocaRepetitionMode.Random)
        {
            if (rep.InputData is null)
                return (false, TranslateRepetitionErrors.InputDataInvalid, null);

            if (rep.InputData.Type == PororocaRepetitionInputDataType.File)
            {
                string resolvedInputDataFilePath = IPororocaVariableResolver.ReplaceTemplates(rep.InputData?.InputFilePath, collectionEffectiveVariables);
                if (!File.Exists(resolvedInputDataFilePath))
                {
                    return (false, TranslateRepetitionErrors.InputDataFileNotFound, null);
                }
            }

            try
            {
                var inputData = await rep.InputData!.ResolveAsync(collectionEffectiveVariables, cancellationToken);

                if (inputData.Length == 0)
                    return (false, TranslateRepetitionErrors.InputDataAtLeastOneLine, null);
                else
                    return (true, null, inputData);
            }
            catch (Exception)
            {
                return (false, TranslateRepetitionErrors.InputDataInvalid, null);
            }
        }
        else
        {
            return (true, null, null);
        }
    }

    private static async Task<PororocaVariable[][]> ResolveAsync(this PororocaRepetitionInputData inputData, IEnumerable<PororocaVariable> colEffectiveVariables, CancellationToken cancellationToken)
    {
        Dictionary<string, string>[] inputLines;

        if (inputData.Type == PororocaRepetitionInputDataType.File)
        {
            string resolvedFilePath = IPororocaVariableResolver.ReplaceTemplates(inputData.InputFilePath, colEffectiveVariables);

            using FileStream fs = new(resolvedFilePath!, FileMode.Open, FileAccess.Read, FileShare.Read, 65536, useAsync: true);
            inputLines = (await JsonSerializer.DeserializeAsync(fs, MinifyingJsonCtx.DictionaryStringStringArray, cancellationToken))!;
        }
        else
        {
            string resolvedRawJsonArray = IPororocaVariableResolver.ReplaceTemplates(inputData.RawJsonArray, colEffectiveVariables);
            inputLines = JsonSerializer.Deserialize(resolvedRawJsonArray!, MinifyingJsonCtx.DictionaryStringStringArray)!;
        }

        return inputLines.Select(il => il.Select(kvp => new PororocaVariable(true, kvp.Key, kvp.Value, true)).ToArray()).ToArray();
    }
}