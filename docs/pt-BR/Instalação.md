# Instalação

## Windows

[Baixe](https://github.com/alexandrehtrb/Pororoca/releases) o arquivo .zip para o seu sistema (`win`), extraia esse arquivo em uma pasta e execute o arquivo `Pororoca.exe`. Se desejar, clique com o botão direito do mouse no arquivo, "Enviar para", "Área de Trabalho (criar atalho)".

## Mac OSX

[Baixe](https://github.com/alexandrehtrb/Pororoca/releases) o arquivo .zip para o seu sistema (`osx`) e extraia esse arquivo em uma pasta.

Depois disso, mova o aplicativo Pororoca para sua pasta Aplicações (Applications). *Sem isso, suas coleções e configurações não permanecerão salvas no seu computador.*

Também é necessário que seu Mac OS autorize programas de desenvolvedores não identificados. Há tutoriais de como autorizar nestes links: [link1](https://macmagazine.com.br/post/2021/09/20/como-instalar-apps-de-desenvolvedores-nao-identificados-no-mac/) e [link2](https://support.apple.com/pt-br/guide/mac-help/mh40616/mac).

*Se estiver usando um Mac OS com chip Apple Silicon M1 e o app de um pacote `osx-arm64` não rodar*, tente baixar e usar o pacote `osx-x64`.

## Linux

[Baixe](https://github.com/alexandrehtrb/Pororoca/releases) o arquivo .zip para o seu sistema (`linux`) e extraia esse arquivo em uma pasta. Depois disso, é só executar o arquivo Pororoca clicando duas vezes nele ou abrindo-o no Terminal.

Se você quiser fazer requisições HTTP/3, o pacote [msquic](https://github.com/microsoft/msquic) deve estar instalado em sua máquina. Há instruções de instalação [aqui](https://docs.microsoft.com/pt-br/aspnet/core/fundamentals/servers/kestrel/http3?view=aspnetcore-6.0#linux) que se aplicam às distribuições Linux mais comuns.

Caso sua distribuição Linux não tenha o pacote msquic disponível, você pode compilá-lo e instalá-lo. O pacote [lttng-tools](https://github.com/giraldeau/lttng-tools) precisa ser instalado e há um tutorial de compilação e instalação [aqui](https://github.com/microsoft/msquic/discussions/2318#discussioncomment-2015375).

