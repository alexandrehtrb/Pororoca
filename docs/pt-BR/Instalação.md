# Instalação

Os pacotes de instalação estão disponíveis na página de [Releases](https://github.com/alexandrehtrb/Pororoca/releases) no GitHub. Esta página é a única fonte oficial e confiável para baixar o programa.

Atualmente, há suporte apenas para arquiteturas `x86` e `x64`. A última versão que suporta arquiteturas `arm` e `arm64` é a 1.5.0. Você pode rodar os executáveis x86 e x64 em uma máquina ARM se o sistema operacional desta suportar emulação.

## Windows (`win`)

*Importante*: Os programas .exe não são assinados, por isso, podem surgir mensagens do Windows SmartScreen dizendo que o programa não é confiável. Basta clicar em "Mais informações", depois em "Executar mesmo assim" para continuar.

### Com instalador (`_installer`)

Baixe o instalador para o seu sistema e siga os passos de instalação.

### Portátil (`_portable`)

Baixe e extraia o pacote, depois execute o arquivo `Pororoca.exe`. Se desejar, clique com o botão direito do mouse no arquivo, "Enviar para", "Área de Trabalho (criar atalho)".

## Mac OSX (`osx`)

Baixe e extraia o pacote, depois, mova o aplicativo Pororoca para sua pasta Aplicações (Applications). *Sem isso, suas coleções e configurações não permanecerão salvas no seu computador.*

Também é necessário que seu Mac OS autorize programas de desenvolvedores não identificados. Há tutoriais de como autorizar nestes links: [link1](https://macmagazine.com.br/post/2021/09/20/como-instalar-apps-de-desenvolvedores-nao-identificados-no-mac/) e [link2](https://support.apple.com/pt-br/guide/mac-help/mh40616/mac).

*Se estiver usando um Mac OS com chip Apple Silicon M1 e o app de um pacote `osx-arm64` não rodar*, tente baixar e usar o pacote `osx-x64`.

## Linux (`linux`)

*Importante*: o Pororoca requer o msquic versão 1.9.0, de modo que usar versões acima dessa acarretará em falha ao efetuar chamadas HTTP/3.

Baixe e extraia o pacote em uma pasta. Depois disso, é só executar o arquivo Pororoca clicando duas vezes nele ou abrindo-o no Terminal.

Se você quiser fazer requisições HTTP/3, o pacote [msquic](https://github.com/microsoft/msquic) deve estar instalado em sua máquina. Há instruções de instalação [aqui](https://docs.microsoft.com/pt-br/aspnet/core/fundamentals/servers/kestrel/http3?view=aspnetcore-6.0#linux) que se aplicam às distribuições Linux mais comuns.

Caso sua distribuição Linux não tenha o pacote msquic disponível, você pode compilá-lo e instalá-lo. O pacote [lttng-tools](https://github.com/giraldeau/lttng-tools) precisa ser instalado e há um tutorial de compilação e instalação [aqui](https://github.com/microsoft/msquic/discussions/2318#discussioncomment-2015375). *Atente-se em usar a versão correta do repositório do msquic:* `git checkout v1.9.0`.