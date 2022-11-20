using Microsoft.Net.Http.Headers;

namespace Pororoca.TestServer.Endpoints;

public static class MultipartRequestHelper
{
    // Content-Type: multipart/form-data; boundary="----WebKitFormBoundarymx2fSWqWSd0OxQqq"
    // The spec at https://tools.ietf.org/html/rfc2046#section-5.1 states that 70 characters is a reasonable limit.
    public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
    {
        string? boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

        if (string.IsNullOrWhiteSpace(boundary))
        {
            throw new InvalidDataException("Missing content-type boundary.");
        }

        if (boundary.Length > lengthLimit)
        {
            throw new InvalidDataException(
                $"Multipart boundary length limit {lengthLimit} exceeded.");
        }

        return boundary;
    }

    public static bool IsMultipartContentType(string? contentType) =>
        !string.IsNullOrEmpty(contentType)
     && contentType.Contains("multipart/", StringComparison.OrdinalIgnoreCase);

    public static bool HasFormDataContentDisposition(ContentDispositionHeaderValue? contentDisposition) =>
        // Content-Disposition: form-data; name="key";
        contentDisposition != null
            && contentDisposition.DispositionType.Equals("form-data")
            && string.IsNullOrEmpty(contentDisposition.FileName.Value)
            && string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);

    public static bool HasFileContentDisposition(ContentDispositionHeaderValue? contentDisposition) =>
        // Content-Disposition: form-data; name="myfile1"; filename="Misc 002.jpg"
        contentDisposition != null
            && contentDisposition.DispositionType.Equals("form-data")
            && (!string.IsNullOrEmpty(contentDisposition.FileName.Value)
                || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
}