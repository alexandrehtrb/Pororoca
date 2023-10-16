namespace Pororoca.Domain.Features.Entities.Pororoca.Http;

public enum PororocaHttpResponseValueCaptureType
{
    Header = 1,
    Body = 2
}

public sealed record PororocaHttpResponseValueCapture(
    PororocaHttpResponseValueCaptureType Type,
    string TargetVariable,
    string? HeaderName,
    string? Path)
{
    // Parameterless constructor for JSON deserialization
    public PororocaHttpResponseValueCapture() : this(PororocaHttpResponseValueCaptureType.Body, string.Empty, null, null) { }

    public PororocaHttpResponseValueCapture Copy() => this with { };
}