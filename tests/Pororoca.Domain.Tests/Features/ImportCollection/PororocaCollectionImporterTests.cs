using Xunit;
using System.Text;
using Pororoca.Domain.Features.Entities.Pororoca;
using static Pororoca.Domain.Features.ImportCollection.PororocaCollectionImporter;

namespace Pororoca.Domain.Tests.Features.ImportCollection;

public static class PororocaCollectionImporterTests
{

    [Fact]
    public static void Should_import_valid_pororoca_collection_correctly()
    {
        // GIVEN
        string json = GetTestFileJson("EmptyCollection.pororoca_collection.json");

        // WHEN AND THEN
        Assert.True(TryImportPororocaCollection(json, out PororocaCollection? col));

        // THEN
        Assert.NotNull(col);
        // Always generating new id, in case user imports the same collection twice
        // This is to avoid overwriting when saving user collections
        Assert.NotEqual(Guid.Parse("ec794541-5c81-49a2-b3d1-113df7432df1"), col!.Id);
        Assert.Equal(DateTimeOffset.Parse("2022-03-03T22:04:15.7115044-03:00"), col.CreatedAt);
        Assert.Equal("Nova coleção", col.Name);
        Assert.NotNull(col.Folders);
        Assert.Empty(col.Folders);
        Assert.NotNull(col.Requests);
        Assert.Empty(col.Requests);
        Assert.NotNull(col.Variables);
        Assert.Empty(col.Variables);
        Assert.NotNull(col.Environments);
        Assert.Empty(col.Environments);
    }

    private static string GetTestFileJson(string fileName)
    {
        DirectoryInfo testDataDirInfo = new DirectoryInfo(Environment.CurrentDirectory).Parent!.Parent!.Parent!;
        string jsonFileInfoPath = Path.Combine(testDataDirInfo.FullName, "TestData", fileName);
        return File.ReadAllText(jsonFileInfoPath, Encoding.UTF8);
    }
}