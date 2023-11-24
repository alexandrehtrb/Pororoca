# Pororoca.Test

This is the official Pororoca package for running automated tests. [Pororoca](https://pororoca.io/) is a tool for HTTP inspection, and this package can be used for HTTP integration tests, or simply for making HTTP requests.

To use it, you will need a Pororoca collection file to include inside your test project. Any .NET test framework can be used: xUnit, MSTest, and others, even console applications.

A full tutorial is available on the [documentation website](https://pororoca.io/docs/automated-tests).

## Example of usage within a test

The code below shows how to use the Pororoca.Test in a xUnit test. First, it loads a Pororoca collection from a file. Then, defines the environment that will be used.

```cs
using Xunit;
using System.Net;
using Pororoca.Test;

namespace Pororoca.Test.Tests;

public class MyPororocaTest
{
    private readonly PororocaTest pororocaTest;

    public MyPororocaTest()
    {
        string filePath = @"C:\Tests\MyCollection.pororoca_collection.json";
        pororocaTest = PororocaTest.LoadCollectionFromFile(filePath)
                                   .AndUseTheEnvironment("Local");
    }

    [Fact]
    public async Task Should_get_JSON_successfully()
    {
        var res = await pororocaTest.SendRequestAsync("Get JSON");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("application/json; charset=utf-8", res.ContentType);
        Assert.Contains("\"id\": 1", res.GetBodyAsText());
    }
}
```

To load a Pororoca collection file that is in your test project folder, you can do it like this:

```cs
private static string GetTestCollectionFilePath()
{
    var testDataDirInfo = new DirectoryInfo(Environment.CurrentDirectory).Parent!.Parent!.Parent!;
    return Path.Combine(testDataDirInfo.FullName, "MyPororocaCollection.pororoca_collection.json");
}
```