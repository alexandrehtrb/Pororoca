namespace Pororoca.Domain.Features.Entities.Pororoca.Http;

public enum PororocaHttpRequestFormDataParamType
{
    Text,
    File
}

public sealed record PororocaHttpRequestFormDataParam(
    bool Enabled,
    PororocaHttpRequestFormDataParamType Type,
    string Key,
    string? TextValue,
    string ContentType,
    string? FileSrcPath
)
{
    // Parameterless constructor for JSON deserialization
    public PororocaHttpRequestFormDataParam() : this(true, PororocaHttpRequestFormDataParamType.Text, string.Empty, string.Empty, string.Empty, null) { }

    public PororocaHttpRequestFormDataParam Copy() => this with { };

    public static PororocaHttpRequestFormDataParam MakeTextParam(bool enabled, string key, string textValue, string contentType) => new(
        enabled,
        PororocaHttpRequestFormDataParamType.Text,
        key,
        textValue,
        contentType,
        null);

    public static PororocaHttpRequestFormDataParam MakeFileParam(bool enabled, string key, string fileSrcPath, string contentType) => new(
        enabled,
        PororocaHttpRequestFormDataParamType.File,
        key,
        null,
        contentType,
        fileSrcPath);
}