# Automated tests

With Pororoca, you can make automated tests with .NET testing tools, such as [xUnit](https://xunit.net/). These tests can be executed in a pipeline or via command line, sending requests to a server.

To create and run these tests, you need to have [.NET](https://dotnet.microsoft.com) 6 or later in your computer.

## Creating the test project

Create a test project through Visual Studio or via command line. For this latter option, type the following command on your console:

```sh
mkdir MyPororocaTest
cd .\MyPororocaTest\
dotnet new xunit
# other testing libraries can be used
```

After that, in the created test project, the .csproj file must be edited to include the [Pororoca.Test](https://www.nuget.org/packages/Pororoca.Test/) NuGet package and to enable experimental features, that, in this case, HTTP/3 will be the one we will use.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <!-- TargetFramework needs to be .net6.0 or higher -->
    <TargetFramework>net6.0</TargetFramework>
    ...
    <!-- EnablePreviewFeatures and RuntimeHostConfigurationOption enable HTTP/3 in the project -->
    <EnablePreviewFeatures>True</EnablePreviewFeatures>
  </PropertyGroup>
  <ItemGroup>
    <RuntimeHostConfigurationOption Include="System.Net.SocketsHttpHandler.Http3Support" Value="true" />
  </ItemGroup>
  <ItemGroup>
    <!-- The line below adds Pororoca.Test package to the project -->
    <PackageReference Include="Pororoca.Test" Version="1.3.0" />
    ...
  </ItemGroup>

</Project>
```

## Making your first test

The code below shows how to use the `Pororoca.Test` in a xUnit test. First, it loads a Pororoca collection from a file. Then, defines the environment that will be used.

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

There are methods in the PororocaTest class to set values of variables during the tests executions. They can be used to set an authentication token, for example:

```cs
pororocaTest.SetCollectionVariable("MyAuthenticationToken", "token_auth");
```

The test project [Pororoca.Test.Tests](https://github.com/alexandrehtrb/Pororoca/tree/master/tests/Pororoca.Test.Tests) in this project can guide you - it shows how to use the `Pororoca.Test` project, how to load the collection file and how to set variables.

## Running the tests

You can run the tests on Visual Studio or executing `dotnet test` through the command line.