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

Após isso, no projeto de testes criado, o arquivo .csproj precisa ser editado para incluir o pacote NuGet [Pororoca.Test](https://www.nuget.org/packages/Pororoca.Test/).

Se você estiver usando .NET 6 em seu projeto de testes, a versão do Pororoca.Test precisa ser 1.x.y. Confira a [documentação de Testes Automatizados](https://github.com/alexandrehtrb/Pororoca/blob/1.6.0/docs/pt-BR/TestesAutomatizados.md) para Pororoca.Test v1, se for seu caso.

Se seus testes serão executados em uma máquina Linux e esses testes incluírem HTTP/3, a biblioteca [msquic](https://github.com/microsoft/msquic) precisa estar instalada. Use msquic v1 para .NET 6 e msquic v2 para .NET 7.


```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <!-- O TargetFramework precisa ser .net6.0 ou acima -->
    <!-- Pororoca.Test v1 usa .net6.0 -->
    <!-- Pororoca.Test v2 usa .net7.0 -->
    <TargetFramework>net7.0</TargetFramework>
    ...
  </PropertyGroup>
  <ItemGroup>
    <!-- A linha abaixo adiciona o pacote Pororoca.Test ao projeto -->
    <PackageReference Include="Pororoca.Test" Version="2.0.0" />
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

O projeto de testes [Pororoca.Test.Tests](https://github.com/alexandrehtrb/Pororoca/tree/master/tests/Pororoca.Test.Tests) pode guiar e servir de base - ele mostra como usar o pacote `Pororoca.Test`, como carregar o arquivo de coleção e como setar variáveis em testes.

## Testes de WebSocket

Você também pode testar WebSockets com o Pororoca. O código abaixo mostra um exemplo de teste:

```cs
[Fact]
public async Task Deve_conectar_e_desconectar_com_sucesso()
{
    // CONECTANDO
    var ws = await this.pororocaTest.ConnectWebSocketAsync("WebSocket HTTP1");
    Assert.Equal(PororocaWebSocketConnectorState.Connected, ws.State);

    // ENVIANDO MENSAGEM DE FECHAMENTO
    var esperarPorEnvio = Task.Delay(TimeSpan.FromSeconds(1));
    var envio = ws.SendMessageAsync("Bye").AsTask();
    await Task.WhenAll(esperarPorEnvio, envio);
    
    // ASSERÇÕES
    Assert.Null(ws.ConnectionException);
    Assert.Equal(PororocaWebSocketConnectorState.Disconnected, ws.State);

    var msg = Assert.IsType<PororocaWebSocketClientMessageToSend>(ws.ExchangedMessages[0]);
    Assert.Equal(PororocaWebSocketMessageType.Close, msg.MessageType);
    Assert.Equal("Adiós", msg.Text);
}
```

## Rodando os testes

Você pode rodar os testes no Visual Studio ou executando `dotnet test` através da linha de comando.