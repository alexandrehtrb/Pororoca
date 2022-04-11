# Testes automatizados

Com o Pororoca, você pode fazer testes automatizados junto com ferramentas de testes do .NET, como o [xUnit](https://xunit.net/). Esses testes podem ser executados em uma pipeline ou via linha de comando, enviando requisições a um servidor.

Para criar e rodar esses testes, é necessário ter o [.NET](https://dotnet.microsoft.com) 6 ou acima em seu computador.

## Criando o projeto de testes

Crie um projeto de testes através do Visual Studio ou via linha de comando. Para este último caso, digite os seguintes comandos no seu console:

```sh
mkdir MeuTestePororoca
cd .\MeuTestePororoca\
dotnet new xunit
# outras bibliotecas de teste podem ser usadas
```

Após isso, no projeto de testes criado, o arquivo .csproj precisa ser editado para incluir o pacote NuGet [Pororoca.Test](https://www.nuget.org/packages/Pororoca.Test/) e para permitir funcionalidades experimentais, que, neste caso, o HTTP/3 é a que iremos utilizar.


```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <!-- O TargetFramework precisa ser .net6.0 ou acima -->
    <TargetFramework>net6.0</TargetFramework>
    ...
    <!-- EnablePreviewFeatures e RuntimeHostConfigurationOption habilitam HTTP/3 no projeto -->
    <EnablePreviewFeatures>True</EnablePreviewFeatures>
  </PropertyGroup>
  <ItemGroup>
    <RuntimeHostConfigurationOption Include="System.Net.SocketsHttpHandler.Http3Support" Value="true" />
  </ItemGroup>
  <ItemGroup>
    <!-- A linha abaixo adiciona o pacote Pororoca.Test ao projeto -->
    <PackageReference Include="Pororoca.Test" Version="1.2.0" />
    ...
  </ItemGroup>

</Project>
```

## Fazendo seu primeiro teste

O código abaixo mostra como usar o `Pororoca.Test` em um teste no xUnit. Primeiro, ele carrega uma coleção Pororoca vinda de um arquivo. Em seguida, ele define o ambiente que será usado.

```cs
using Xunit;
using System.Net;
using Pororoca.Test;

namespace Pororoca.Test.Tests;

public class MeuTestePororoca
{
    private readonly PororocaTest pororocaTest;

    public MeuTestePororoca()
    {
        string caminhoArquivo = @"C:\Testes\MinhaColecao.pororoca_collection.json";
        pororocaTest = PororocaTest.LoadCollectionFromFile(caminhoArquivo)
                                   .AndUseTheEnvironment("Local");
    }

    [Fact]
    public async Task Deve_obter_JSON_com_sucesso()
    {
        var res = await pororocaTest.SendRequestAsync("Get JSON");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("application/json; charset=utf-8", res.ContentType);
        Assert.Contains("\"id\": 1", res.GetBodyAsText());
    }
}
```

Há métodos na classe PororocaTest para setar valores de variáveis durante a execução dos testes. Eles podem ser usados para setar um token de autenticação, por exemplo:

```cs
pororocaTest.SetCollectionVariable("MeuTokenAutenticacao", "token_auth");
```

O projeto de testes [Pororoca.Test.Tests](https://github.com/alexandrehtrb/Pororoca/tree/master/tests/Pororoca.Test.Tests) neste repositório pode guiar e servir de base - ele mostra como usar o pacote `Pororoca.Test`, como carregar o arquivo de coleção e como setar variáveis em testes.

## Rodando os testes

Você pode rodar os testes no Visual Studio ou executando `dotnet test` através da linha de comando.