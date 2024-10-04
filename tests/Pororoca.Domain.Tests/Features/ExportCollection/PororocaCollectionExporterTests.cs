using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Xunit;
using static Pororoca.Domain.Features.ExportCollection.PororocaCollectionExporter;
using static Pororoca.Domain.Features.ImportCollection.PororocaCollectionImporter;

namespace Pororoca.Domain.Tests.Features.ExportCollection;

public static class PororocaCollectionExporterTests
{
    private static readonly Guid testGuid = Guid.NewGuid();
    private const string testName = "MyCollection";

    [Fact]
    public static void Should_export_and_reimport_pororoca_collection_successfully()
    {
        // GIVEN
        string json1 = ReadTestFileText("FullCollection.pororoca_collection.json");

        // WHEN AND THEN
        Assert.True(TryImportPororocaCollection(json1, preserveId: true, out var col));

        // THEN
        Assert.NotNull(col);

        // WHEN AND THEN
        string minifiedInputJson = MinifyJsonString(json1);
        string minifiedOutputJson = MinifyJsonString(ExportAsPororocaCollection(col));
        Assert.Equal(minifiedOutputJson, minifiedInputJson);
    }

    [Fact]
    public static void Should_export_and_reimport_pororoca_collection_successfully_2()
    {
        // GIVEN
        var col = CreateTestCollection();

        // WHEN AND THEN
        string json = ExportAsPororocaCollection(col);
        Assert.True(TryImportPororocaCollection(json, preserveId: true, out var colReimported));
        Assert.NotNull(colReimported);
        AssertCollection(colReimported);
    }

    private static PororocaCollection CreateTestCollection()
    {
        PororocaCollection col = new(testGuid, testName, DateTimeOffset.Now);
        PororocaHttpRequest req1 = new("Req1", Url: "http://www.abc.com.br");
        PororocaHttpRequest req2 = new("Req2", Url: "https://www.ghi.com.br");
        PororocaCollectionFolder folder1 = new("Folder1");
        folder1.Requests.Add(req2);
        col.Requests.Add(req1);
        col.Folders.Add(folder1);
        col.Variables.AddRange(new PororocaVariable[]
        {
            new(true, "Key1", "Value1", false),
            new(false, "Key2", "Value2", true)
        });
        var env1 = new PororocaEnvironment("MyEnvironment") with
        {
            IsCurrent = true,
            Variables = [
                new(true, "Key3", "Value3", true),
                new(true, "Key4", "Value4", false)]
        };
        col.Environments.Add(env1);
        return col;
    }

    private static void AssertCollection(PororocaCollection col)
    {
        Assert.NotNull(col);
        Assert.Equal(testGuid, col.Id);
        Assert.Equal(testName, col.Name);
        Assert.Equal(2, col.Variables.Count);
        var env1 = Assert.Single(col.Environments);
        var folder1 = Assert.Single(col.Folders);
        Assert.Single(col.Requests);

        var req1 = Assert.Single(col.HttpRequests);
        Assert.Equal("Req1", req1.Name);
        Assert.Equal("GET", req1.HttpMethod);
        Assert.Equal("http://www.abc.com.br", req1.Url);

        Assert.Equal("Folder1", folder1.Name);
        Assert.Empty(folder1.Folders);
        Assert.Single(folder1.Requests);

        var req2 = folder1.HttpRequests[0];
        Assert.Equal("Req2", req2.Name);
        Assert.Equal("GET", req2.HttpMethod);
        Assert.Equal("https://www.ghi.com.br", req2.Url);

        Assert.Equal(2, col.Variables.Count);

        var var1 = col.Variables[0];
        Assert.True(var1.Enabled);
        Assert.Equal("Key1", var1.Key);
        Assert.Equal("Value1", var1.Value);
        Assert.False(var1.IsSecret);

        var var2 = col.Variables[1];
        Assert.False(var2.Enabled);
        Assert.Equal("Key2", var2.Key);
        Assert.True(var2.IsSecret);
        Assert.Equal("Value2", var2.Value);

        Assert.True(env1.IsCurrent); // Should preserve environment.IsCurrent when exporting it inside of a collection

        var var3 = env1.Variables[0];
        Assert.True(var3.Enabled);
        Assert.Equal("Key3", var3.Key);
        Assert.True(var3.IsSecret);
        Assert.Equal("Value3", var3.Value);

        var var4 = env1.Variables[1];
        Assert.True(var4.Enabled);
        Assert.Equal("Key4", var4.Key);
        Assert.Equal("Value4", var4.Value);
        Assert.False(var4.IsSecret);
    }
}