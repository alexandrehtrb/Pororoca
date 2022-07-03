using Pororoca.Domain.Features.Common;
using Xunit;

namespace Pororoca.Domain.Tests.Features.Common;

public static class FileNameUtilsTests
{
    [Theory]
    [InlineData("xls", "C:\\Folder1\arq1.xls")]
    [InlineData("txt", "C:\\Folder1\\Folder2\\arq2.txt")]
    [InlineData("jpg", "/usr/bin/img1.jpg")]
    [InlineData("jpg", "img1.jpg")]
    public static void Should_return_file_extension_without_dot_from_file_path(string expectedFileExtensionWithoutDot, string fileNameOrPath) =>
        Assert.Equal(expectedFileExtensionWithoutDot, FileNameUtils.GetFileExtensionWithoutDot(fileNameOrPath));


    [Theory]
    [InlineData("C:\\Folder1\arq1")]
    [InlineData("/usr/bin/arq1")]
    public static void If_file_does_not_have_extension_then_return_null(string fileNameOrPath) =>
        Assert.Null(FileNameUtils.GetFileExtensionWithoutDot(fileNameOrPath));
}