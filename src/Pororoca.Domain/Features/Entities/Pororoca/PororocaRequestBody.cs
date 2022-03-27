using System.Text.Json.Serialization;

namespace Pororoca.Domain.Features.Entities.Pororoca;

public sealed class PororocaRequestBody : ICloneable
{
    [JsonInclude]
    public PororocaRequestBodyMode Mode { get; private set; }

    [JsonInclude]
    public string? ContentType { get; private set; }

    [JsonInclude]
    public string? RawContent { get; private set; }

    [JsonInclude]
    public string? FileSrcPath { get; private set; }

    [JsonInclude]
    public IReadOnlyList<PororocaKeyValueParam>? UrlEncodedValues { get; private set; }

    [JsonInclude]
    public IReadOnlyList<PororocaRequestFormDataParam>? FormDataValues { get; private set; }

    [JsonInclude]
    public PororocaRequestBodyGraphQl? GraphQlValues { get; private set; }
    
    public PororocaRequestBody()
    {
        Mode = PororocaRequestBodyMode.Raw;
        ContentType = null;
        RawContent = null;
        FileSrcPath = null;
        UrlEncodedValues = null;
        FormDataValues = null;
        GraphQlValues = null;
    }

    public void SetRawContent(string rawContent, string contentType)
    {
        Mode = PororocaRequestBodyMode.Raw;
        ContentType = contentType;
        RawContent = rawContent;
    }

    public void SetFileContent(string filePath, string contentType)
    {
        Mode = PororocaRequestBodyMode.File;
        ContentType = contentType;
        FileSrcPath = filePath;
    }

    public void SetUrlEncodedContent(IEnumerable<PororocaKeyValueParam> urlEncodedValues)
    {
        Mode = PororocaRequestBodyMode.UrlEncoded;
        UrlEncodedValues = urlEncodedValues.ToList().AsReadOnly();
    }

    public void SetFormDataContent(IEnumerable<PororocaRequestFormDataParam> formDataValues)
    {
        Mode = PororocaRequestBodyMode.FormData;
        FormDataValues = formDataValues.ToList().AsReadOnly();
    }

    public void SetGraphQlContent(string? query, string? variables)
    {
        Mode = PororocaRequestBodyMode.GraphQl;
        GraphQlValues = new(query, variables);
    }

    public object Clone() =>
        new PororocaRequestBody()
        {
            Mode = Mode,
            ContentType = ContentType,
            RawContent = RawContent,
            FileSrcPath = FileSrcPath,
            UrlEncodedValues = UrlEncodedValues?.Select(u => (PororocaKeyValueParam)u.Clone())?.ToList()?.AsReadOnly(),
            FormDataValues = FormDataValues?.Select(f => (PororocaRequestFormDataParam)f.Clone())?.ToList()?.AsReadOnly(),
            GraphQlValues = GraphQlValues?.Clone()
        };
}