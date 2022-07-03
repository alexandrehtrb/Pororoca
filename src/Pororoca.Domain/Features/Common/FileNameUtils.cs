namespace Pororoca.Domain.Features.Common;

internal static class FileNameUtils
{
    internal static string? GetFileExtensionWithoutDot(string fileNameOrPath)
    {
        int lastDotIndex = fileNameOrPath.LastIndexOf('.');
        return lastDotIndex != -1 ? fileNameOrPath[(lastDotIndex + 1)..] : null;
    }
}