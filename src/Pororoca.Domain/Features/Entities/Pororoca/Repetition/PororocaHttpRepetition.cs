using System.Text;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using static Pororoca.Domain.Feature.Entities.Pororoca.Repetition.PororocaRepetitionInputData;

namespace Pororoca.Domain.Feature.Entities.Pororoca.Repetition;

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

public sealed class PororocaHttpRepetition : PororocaRequest
{
    public string BaseRequestPath { get; set; }
    public PororocaRepetitionMode RepetitionMode { get; set; }
    public int? NumberOfRepetitions { get; set; } // only for Simple and Random
    public int? MaxDop { get; set; } // only for Simple and Random
    public int? DelayInMs { get; set; }
    public PororocaRepetitionInputData? InputData { get; set; } // only for Sequential and Random

#nullable disable warnings
    public PororocaHttpRepetition() : this(string.Empty)
    {
        // Parameterless constructor for JSON deserialization
    }
#nullable restore warnings

    public PororocaHttpRepetition(string name, string? inputDataRawComment = null) : base(PororocaRequestType.HttpRepetition, name)
    {
        BaseRequestPath = string.Empty;
        RepetitionMode = PororocaRepetitionMode.Sequential;
        NumberOfRepetitions = null;
        MaxDop = null;
        DelayInMs = null;
        InputData = inputDataRawComment is null ?
            null : MakeRawJsonInputDataExample(inputDataRawComment);
    }

    public override object Clone() => new PororocaHttpRepetition(Name)
    {
        BaseRequestPath = this.BaseRequestPath,
        RepetitionMode = this.RepetitionMode,
        NumberOfRepetitions = this.NumberOfRepetitions,
        MaxDop = this.MaxDop,
        DelayInMs = this.DelayInMs,
        InputData = this.InputData
    };
}

public sealed record PororocaHttpRepetitionResult(
    PororocaVariable[]? InputLine,
    PororocaHttpResponse? Response,
    string? ValidationErrorCode)
{
    public bool Successful => ValidationErrorCode is null && Response?.Successful == true;
}