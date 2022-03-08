using Xunit;
using Pororoca.Domain.Features.Common;

namespace Pororoca.Domain.Tests.Features.Common;

public static class MimeTypesDetectorTests
{
    [Theory]
    [InlineData("application/excel", "C:\\Folder1\\arq1.xls")]
    [InlineData("text/plain", "C:\\arq1.txt")]
    [InlineData("image/jpeg", "/usr/bin/img1.jpg")]
    public static void Should_find_mime_type_for_file_extension_declared_in_mapping(string expectedMimeType, string fileNameOrPath)
    {
        Assert.True(MimeTypesDetector.TryFindMimeTypeForFile(fileNameOrPath, out string? mimeType));
        Assert.Equal(expectedMimeType, mimeType);
    }

    [Fact]
    public static void Should_not_find_mime_type_for_file_extension_not_declared_in_mapping()
    {
        Assert.False(MimeTypesDetector.TryFindMimeTypeForFile("C:\\arq1.jhm", out string? mimeType));
        Assert.Null(mimeType);
    }

    [Theory]
    [InlineData("svg", "image/svg+xml")]
    [InlineData("txt", "text/plain")]
    [InlineData("txt", "text/plain; charset=utf-8")]
    [InlineData("gif", "image/gif")]
    public static void Should_find_file_extension_for_mime_type_declared_in_mapping(string expectedFileExtension, string contentType)
    {
        Assert.True(MimeTypesDetector.TryFindFileExtensionForContentType(contentType, out string? fileExtension));
        Assert.Equal(expectedFileExtension, fileExtension);
    }

    [Fact]
    public static void Should_not_find_file_extension_for_content_type_not_declared_in_mapping()
    {
        Assert.False(MimeTypesDetector.TryFindFileExtensionForContentType("application/unknown", out string? fileExtension));
        Assert.Null(fileExtension);
    }

    [Theory]
    [InlineData("text/json")]
    [InlineData("application/json")]
    [InlineData("application/json; charset=utf-8")]
    public static void Should_detect_json_content_when_content_type_is_json(string contentType) =>
        Assert.True(MimeTypesDetector.IsJsonContent(contentType));

    [Theory]
    [InlineData("text/plain")]
    [InlineData("application/ecmascript")]
    [InlineData("application/javascript; charset=utf-8")]
    public static void Should_detect_that_is_not_json_content_when_content_type_is_not_json(string contentType) =>
        Assert.False(MimeTypesDetector.IsJsonContent(contentType));

    [Theory]
    [InlineData("text/plain")]
    [InlineData("application/json")]
    [InlineData("text/xml")]
    public static void Should_detect_text_content_when_content_type_is_text(string contentType) =>
        Assert.True(MimeTypesDetector.IsTextContent(contentType));

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("application/pdf")]
    [InlineData("application/mspowerpoint")]
    public static void Should_detect_that_is_not_text_content_when_content_type_is_not_text(string contentType) =>
        Assert.False(MimeTypesDetector.IsTextContent(contentType));

}