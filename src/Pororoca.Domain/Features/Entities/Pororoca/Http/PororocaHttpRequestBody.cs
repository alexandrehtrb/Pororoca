namespace Pororoca.Domain.Features.Entities.Pororoca.Http;

public enum PororocaHttpRequestBodyMode
{
    Raw,
    File,
    UrlEncoded,
    FormData,
    GraphQl
}

public sealed record PororocaHttpRequestBody
(
    PororocaHttpRequestBodyMode Mode,
    string? ContentType = null,
    string? RawContent = null,
    string? FileSrcPath = null,
    List<PororocaKeyValueParam>? UrlEncodedValues = null,
    List<PororocaHttpRequestFormDataParam>? FormDataValues = null,
    PororocaHttpRequestBodyGraphQl? GraphQlValues = null
)
{
    // Parameterless constructor for JSON deserialization
    public PororocaHttpRequestBody() : this(PororocaHttpRequestBodyMode.Raw){}

    public static PororocaHttpRequestBody MakeRawContent(string rawContent, string contentType) =>
        new(Mode: PororocaHttpRequestBodyMode.Raw,
            ContentType: contentType,
            RawContent: rawContent);

    public static PororocaHttpRequestBody MakeFileContent(string filePath, string contentType) =>
        new(Mode: PororocaHttpRequestBodyMode.File,
            ContentType: contentType,
            FileSrcPath: filePath);

    public static PororocaHttpRequestBody MakeUrlEncodedContent(IEnumerable<PororocaKeyValueParam> urlEncodedValues) =>
        new(Mode: PororocaHttpRequestBodyMode.UrlEncoded,
            UrlEncodedValues: urlEncodedValues.ToList());

    public static PororocaHttpRequestBody MakeFormDataContent(IEnumerable<PororocaHttpRequestFormDataParam> formDataValues) =>
        new(Mode: PororocaHttpRequestBodyMode.FormData,
            FormDataValues: formDataValues.ToList());

    public static PororocaHttpRequestBody MakeGraphQlContent(string? query, string? variables) =>
        new(Mode: PororocaHttpRequestBodyMode.GraphQl,
            GraphQlValues: new(query, variables));

    public PororocaHttpRequestBody Copy() => this with
    {
        UrlEncodedValues = UrlEncodedValues?.Select(u => u.Copy())?.ToList(),
        FormDataValues = FormDataValues?.Select(f => f.Copy())?.ToList(),
        GraphQlValues = GraphQlValues?.Copy()
    };
}