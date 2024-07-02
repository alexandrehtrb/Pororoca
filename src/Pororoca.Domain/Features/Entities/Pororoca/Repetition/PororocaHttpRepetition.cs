using System.Net;
using System.Text;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using static Pororoca.Domain.Features.Entities.Pororoca.Repetition.PororocaRepetitionInputData;

namespace Pororoca.Domain.Features.Entities.Pororoca.Repetition;

public enum PororocaRepetitionMode
{
    /// <summary>
    /// Fixed number of requests, no input data.
    /// </summary>
    Simple,
    /// <summary>
    /// Use input data sequentially until all input lines are used. No need to specify number of requests. MaxDOP doesn't apply here (not parallel).
    /// </summary>
    Sequential,
    /// <summary>
    /// Fixed number of requests and for each request, randomly select an input line from input data.
    /// </summary>
    Random
}

public enum PororocaRepetitionInputDataType
{
    RawJsonArray,
    File
}

public sealed record PororocaRepetitionInputData(
    PororocaRepetitionInputDataType Type,
    string? RawJsonArray,
    string? InputFilePath)
{
    internal static PororocaRepetitionInputData MakeRawJsonInputDataExample(string inputDataRawComment)
    {
        StringBuilder sb = new();
        sb.AppendLine("[");
        sb.AppendLine("  // " + inputDataRawComment);
        sb.AppendLine("  {\"Var1\":\"ABC\", \"Var2\":\"{{KEY_123}}\"},");
        sb.AppendLine("  {\"Var1\":\"DEF\", \"Var2\":\"456\"},");
        sb.AppendLine("  {\"Var1\":\"GHI\", \"Var2\":\"789\"},");
        sb.AppendLine("]");
        return new(PororocaRepetitionInputDataType.RawJsonArray, sb.ToString(), null);
    }
}

public sealed record PororocaHttpRepetition
(
    string Name,
    string BaseRequestPath,
    PororocaRepetitionMode RepetitionMode,
    int? NumberOfRepetitions, // only for Simple and Random
    int? MaxDop, // only for Simple and Random
    int? DelayInMs,
    PororocaRepetitionInputData? InputData // only for Sequential and Random
) : PororocaRequest(PororocaRequestType.HttpRepetition, Name)
{
    public PororocaHttpRepetition(string name, string? inputDataRawComment = null) : this(
        Name: name,
        BaseRequestPath: string.Empty,
        RepetitionMode: PororocaRepetitionMode.Sequential,
        NumberOfRepetitions: null,
        MaxDop: null,
        DelayInMs: null,
        InputData: inputDataRawComment is null ? null : MakeRawJsonInputDataExample(inputDataRawComment))
    { }

#nullable disable warnings
    public PororocaHttpRepetition() : this(string.Empty)
    {
        // Parameterless constructor for JSON deserialization
    }
#nullable restore warnings

    public override PororocaRequest CopyAbstract() => Copy();

    public PororocaHttpRepetition Copy() => this with { };
}

public sealed record PororocaHttpRepetitionResult(
    PororocaVariable[]? InputLine,
    PororocaHttpResponse? Response,
    string? ValidationErrorCode)
{
    public bool Successful => ValidationErrorCode is null && Response?.Successful == true;

    public bool HasHttp2xxStatusCode
    {
        get
        {
            if (!Successful)
                return false;

            if (Response?.StatusCode is HttpStatusCode sc)
            {
                int sci = (int)sc;
                return 200 <= sci && sci < 300;
            }
            else
                return false;
        }
    }
}