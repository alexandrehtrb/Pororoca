using System.Text.Json.Serialization;

namespace Pororoca.Domain.Features.Entities.Pororoca;

public enum PororocaRequestFormDataParamType
{
    Text,
    File
}

public sealed class PororocaRequestFormDataParam : ICloneable
{
    public bool Enabled { get; set; }

    [JsonInclude]
    public PororocaRequestFormDataParamType Type { get; private set; }

    [JsonInclude]
    public string Key { get; init; }

    [JsonInclude]
    public string? TextValue { get; private set; }

    [JsonInclude]
    public string ContentType { get; private set; }

    [JsonInclude]
    public string? FileSrcPath { get; private set; }

#nullable disable warnings
    public PororocaRequestFormDataParam() : this(true, string.Empty)
    {
        // Parameterless constructor for JSON deserialization
    }
#nullable restore warnings

    public PororocaRequestFormDataParam(bool enabled, string key)
    {
        Enabled = enabled;
        Type = PororocaRequestFormDataParamType.Text;
        Key = key;
        TextValue = null;
        ContentType = string.Empty;
    }

    public void SetTextValue(string textValue, string contentType)
    {
        Type = PororocaRequestFormDataParamType.Text;
        TextValue = textValue;
        ContentType = contentType;
    }

    public void SetFileValue(string fileSrcPath, string contentType)
    {
        Type = PororocaRequestFormDataParamType.File;
        FileSrcPath = fileSrcPath;
        ContentType = contentType;
    }

    public object Clone() =>
        new PororocaRequestFormDataParam()
        {
            Enabled = Enabled,
            Type = Type,
            Key = Key,
            TextValue = TextValue,
            ContentType = ContentType,
            FileSrcPath = FileSrcPath
        };
}