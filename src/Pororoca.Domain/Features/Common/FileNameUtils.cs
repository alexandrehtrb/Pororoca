namespace Pororoca.Domain.Features.Common;

public static class FileNameUtils
{
    public static string? GetFileExtensionWithoutDot(string fileNameOrPath)
    {
        int lastDotIndex = fileNameOrPath.LastIndexOf('.');
        return lastDotIndex != -1 ? fileNameOrPath.Substring(lastDotIndex + 1) : null;
    }
}