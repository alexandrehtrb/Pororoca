global using static Pororoca.Test.Tests.TestFilesLoader;

namespace Pororoca.Test.Tests;

internal static class TestFilesLoader
{
    internal static string GetTestCollectionFilePath()
    {
        var testDataDirInfo = new DirectoryInfo(Environment.CurrentDirectory).Parent!.Parent!.Parent!;
        return Path.Combine(testDataDirInfo.FullName, "PororocaIntegrationTestCollection.pororoca_collection.json");
    }

    internal static string GetTestClientCertificatesDir()
    {
        var testDataDirInfo = new DirectoryInfo(Environment.CurrentDirectory).Parent!.Parent!.Parent!;
        return Path.Combine(testDataDirInfo.FullName, "BadSslClientCertificates");
    }

    internal static string GetTestFilesDir()
    {
        var testDataDirInfo = new DirectoryInfo(Environment.CurrentDirectory).Parent!.Parent!.Parent!;
        return Path.Combine(testDataDirInfo.FullName, "TestFiles");
    }
}