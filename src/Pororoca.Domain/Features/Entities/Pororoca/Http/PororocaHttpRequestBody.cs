using System.Text.Json.Serialization;

namespace Pororoca.Domain.Features.Entities.Pororoca.Http;

public enum PororocaHttpRequestBodyMode
{
    Raw,
    File,
    UrlEncoded,
    FormData,
    GraphQl
}

public sealed class PororocaHttpRequestBody : ICloneable
{
    [JsonInclude]
    public PororocaHttpRequestBodyMode Mode { get; private set; }

    [JsonInclude]
    public string? ContentType { get; private set; }

    [JsonInclude]
    public string? RawContent { get; private set; }

    [JsonInclude]
    public string? FileSrcPath { get; private set; }

    [JsonInclude]
    public List<PororocaKeyValueParam>? UrlEncodedValues { get; private set; }

    [JsonInclude]
    public List<PororocaHttpRequestFormDataParam>? FormDataValues { get; private set; }

    [JsonInclude]
    public PororocaHttpRequestBodyGraphQl? GraphQlValues { get; private set; }

    public PororocaHttpRequestBody()
    {
        Mode = PororocaHttpRequestBodyMode.Raw;
        ContentType = null;
        RawContent = null;
        FileSrcPath = null;
        UrlEncodedValues = null;
        FormDataValues = null;
        GraphQlValues = null;
    }

    public void SetRawContent(string rawContent, string contentType)
    {
        Mode = PororocaHttpRequestBodyMode.Raw;
        ContentType = contentType;
        RawContent = rawContent;
    }

    public void SetFileContent(string filePath, string contentType)
    {
        Mode = PororocaHttpRequestBodyMode.File;
        ContentType = contentType;
        FileSrcPath = filePath;
    }

    public void SetUrlEncodedContent(IEnumerable<PororocaKeyValueParam> urlEncodedValues)
    {
        Mode = PororocaHttpRequestBodyMode.UrlEncoded;
        UrlEncodedValues = urlEncodedValues.ToList();
    }

    public void SetFormDataContent(IEnumerable<PororocaHttpRequestFormDataParam> formDataValues)
    {
        Mode = PororocaHttpRequestBodyMode.FormData;
        FormDataValues = formDataValues.ToList();
    }

    public void SetGraphQlContent(string? query, string? variables)
    {
        Mode = PororocaHttpRequestBodyMode.GraphQl;
        GraphQlValues = new(query, variables);
    }

    public object Clone() =>
        new PororocaHttpRequestBody()
        {
            Mode = Mode,
            ContentType = ContentType,
            RawContent = RawContent,
            FileSrcPath = FileSrcPath,
            UrlEncodedValues = UrlEncodedValues?.Select(u => u.Copy())?.ToList(),
            FormDataValues = FormDataValues?.Select(f => f.Copy())?.ToList(),
            GraphQlValues = GraphQlValues?.Copy()
        };
}