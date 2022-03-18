using Xunit;
using static Pororoca.Domain.Features.ExportCollection.PororocaCollectionExporter;
using Pororoca.Domain.Features.Entities.Pororoca;
using System;

namespace Pororoca.Domain.Tests.Features.ExportCollection;

public static class PororocaCollectionExporterTests
{
    private static readonly Guid testGuid = Guid.NewGuid();
    private const string testName = "MyCollection";

    [Fact]
    public static void Should_hide_pororoca_collection_and_environment_secrets()
    {  
        // GIVEN
        PororocaCollection col = CreateTestCollection();

        // WHEN
        PororocaCollection colWithHiddenSecrets = GenerateCollectionToExport(col, true);

        // THEN
        AssertCollection(colWithHiddenSecrets, true);
    }

    [Fact]
    public static void Should_not_hide_pororoca_collection_and_environment_secrets()
    {  
        // GIVEN
        PororocaCollection col = CreateTestCollection();

        // WHEN
        PororocaCollection colWithHiddenSecrets = GenerateCollectionToExport(col, false);

        // THEN
        AssertCollection(colWithHiddenSecrets, false);
    }

    private static PororocaCollection CreateTestCollection()
    {
        PororocaCollection col = new(testGuid, testName, DateTimeOffset.Now);
        PororocaRequest req1 = new("Req1");
        req1.UpdateUrl("http://www.abc.com.br");
        PororocaRequest req2 = new("Req2");
        req2.UpdateUrl("https://www.ghi.com.br");
        PororocaCollectionFolder folder1 = new("Folder1");
        folder1.AddRequest(req2);
        col.AddRequest(req1);
        col.AddFolder(folder1);
        col.UpdateVariables(new PororocaVariable[]
        {
            new(true, "Key1", "Value1", false),
            new(false, "Key2", "Value2", true)
        });
        PororocaEnvironment env1 = new("MyEnvironment");
        env1.UpdateVariables(new PororocaVariable[]
        {
            new(true, "Key3", "Value3", true),
            new(true, "Key4", "Value4", false)
        });
        col.AddEnvironment(env1);
        return col;
    }

    private static void AssertCollection(PororocaCollection col, bool areSecretsHidden)
    {
        Assert.NotNull(col);
        Assert.Equal(testGuid, col.Id);
        Assert.Equal(testName, col.Name);
        Assert.Equal(1, col.Folders.Count);
        Assert.Equal(1, col.Requests.Count);
        Assert.Equal(1, col.Environments.Count);
        Assert.Equal(2, col.Variables.Count);

        


        PororocaRequest req1 = col.Requests[0];
        Assert.Equal("Req1", req1.Name);
        Assert.Equal("GET", req1.HttpMethod);
        Assert.Equal("http://www.abc.com.br", req1.Url);

        PororocaCollectionFolder folder1 = col.Folders[0];
        Assert.Equal("Folder1", folder1.Name);
        Assert.Empty(folder1.Folders);
        Assert.Single(folder1.Requests);

        PororocaRequest req2 = folder1.Requests[0];
        Assert.Equal("Req2", req2.Name);
        Assert.Equal("GET", req2.HttpMethod);
        Assert.Equal("https://www.ghi.com.br", req2.Url);

        Assert.Equal(2, col.Variables.Count);

        PororocaVariable var1 = col.Variables[0];
        Assert.True(var1.Enabled);
        Assert.Equal("Key1", var1.Key);
        Assert.Equal("Value1", var1.Value);
        Assert.False(var1.IsSecret);

        PororocaVariable var2 = col.Variables[1];
        Assert.False(var2.Enabled);
        Assert.Equal("Key2", var2.Key);
        Assert.True(var2.IsSecret);
        if (areSecretsHidden)
        {
            Assert.Equal(string.Empty, var2.Value);
        }
        else
        {
            Assert.Equal("Value2", var2.Value);
        }

        PororocaEnvironment env1 = col.Environments[0];

        PororocaVariable var3 = env1.Variables[0];
        Assert.True(var3.Enabled);
        Assert.Equal("Key3", var3.Key);
        Assert.True(var3.IsSecret);
        if (areSecretsHidden)
        {
            Assert.Equal(string.Empty, var3.Value);
        }
        else
        {
            Assert.Equal("Value3", var3.Value);
        }

        PororocaVariable var4 = env1.Variables[1];
        Assert.True(var4.Enabled);
        Assert.Equal("Key4", var4.Key);
        Assert.Equal("Value4", var4.Value);
        Assert.False(var4.IsSecret);
    }
}