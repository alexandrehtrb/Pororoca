global using static Pororoca.Domain.Tests.TestFilesLoader;

using System.Text;

namespace Pororoca.Domain.Tests;

internal static class TestFilesLoader
{
    internal static string GetTestFilePath(string fileName)
    {
        var testDataDirInfo = new DirectoryInfo(Environment.CurrentDirectory).Parent!.Parent!.Parent!;
        return Path.Combine(testDataDirInfo.FullName, "TestData", fileName);
    }

    internal static string ReadTestFileText(string fileName)
    {
        var testDataDirInfo = new DirectoryInfo(Environment.CurrentDirectory).Parent!.Parent!.Parent!;
        string path = Path.Combine(testDataDirInfo.FullName, "TestData", fileName);
        return File.ReadAllText(path, Encoding.UTF8);
    }

    internal static string ReadTestFileText(string subfolder, string fileName)
    {
        var testDataDirInfo = new DirectoryInfo(Environment.CurrentDirectory).Parent!.Parent!.Parent!;
        string path = Path.Combine(testDataDirInfo.FullName, "TestData", subfolder, fileName);
        return File.ReadAllText(path, Encoding.UTF8);
    }
}