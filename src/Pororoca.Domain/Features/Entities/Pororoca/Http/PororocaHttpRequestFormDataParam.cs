using System.Text.Json.Serialization;

namespace Pororoca.Domain.Features.Entities.Pororoca.Http;

public enum PororocaHttpRequestFormDataParamType
{
    Text,
    File
}

public sealed class PororocaHttpRequestFormDataParam : ICloneable
{
    public bool Enabled { get; set; }

    [JsonInclude]
    public PororocaHttpRequestFormDataParamType Type { get; private set; }

    [JsonInclude]
    public string Key { get; init; }

    [JsonInclude]
    public string? TextValue { get; private set; }

    [JsonInclude]
    public string ContentType { get; private set; }

    [JsonInclude]
    public string? FileSrcPath { get; private set; }

#nullable disable warnings
    public PororocaHttpRequestFormDataParam() : this(true, string.Empty)
    {
        // Parameterless constructor for JSON deserialization
    }
#nullable restore warnings

    public PororocaHttpRequestFormDataParam(bool enabled, string key)
    {
        Enabled = enabled;
        Type = PororocaHttpRequestFormDataParamType.Text;
        Key = key;
        TextValue = null;
        ContentType = string.Empty;
    }

    public void SetTextValue(string textValue, string contentType)
    {
        Type = PororocaHttpRequestFormDataParamType.Text;
        TextValue = textValue;
        ContentType = contentType;
    }

    public void SetFileValue(string fileSrcPath, string contentType)
    {
        Type = PororocaHttpRequestFormDataParamType.File;
        FileSrcPath = fileSrcPath;
        ContentType = contentType;
    }

    public object Clone() =>
        new PororocaHttpRequestFormDataParam()
        {
            Enabled = Enabled,
            Type = Type,
            Key = Key,
            TextValue = TextValue,
            ContentType = ContentType,
            FileSrcPath = FileSrcPath
        };
}