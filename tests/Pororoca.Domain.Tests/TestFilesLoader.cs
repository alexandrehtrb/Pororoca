global using static Pororoca.Domain.Tests.TestFilesLoader;

using System.Text;
using System.Text.Json;
using static Pororoca.Domain.Features.Common.JsonConfiguration;

namespace Pororoca.Domain.Tests;

internal static class TestFilesLoader
{
    internal static string GetTestFilesDirPath()
    {
        var testDataDirInfo = new DirectoryInfo(Environment.CurrentDirectory).Parent!.Parent!.Parent!;
        return Path.Combine(testDataDirInfo.FullName, "TestData");
    }

    internal static string GetTestFilePath(string fileName) =>
        Path.Combine(GetTestFilesDirPath(), fileName);

    internal static string ReadTestFileText(string fileName)
    {
        string path = GetTestFilePath(fileName);
        return File.ReadAllText(path, Encoding.UTF8);
    }

    internal static string ReadTestFileText(string subfolder, string fileName)
    {
        string path = Path.Combine(GetTestFilesDirPath(), subfolder, fileName);
        return File.ReadAllText(path, Encoding.UTF8);
    }

    internal static string MinifyJsonString(string json) =>
        JsonSerializer.Serialize(JsonSerializer.Deserialize(json, MinifyingJsonCtx.JsonDocument), MinifyingJsonCtx.JsonDocument!)!;
}