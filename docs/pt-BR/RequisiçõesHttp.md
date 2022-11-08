# Requisições

* [Aspectos gerais](#aspectos-gerais)
* [Criando](#criando)
* [Renomeando](#renomeando)
* [Copiando, colando e excluindo](#copiando-colando-e-excluindo)
* [Autenticação customizada](#autenticação-customizada)
    * [Autenticação Basic](#autenticação-basic)
    * [Autenticação Bearer](#autenticação-bearer)
    * [Autenticação por certificado de cliente](#autenticação-por-certificado-de-cliente)
* [Enviando uma requisição](#enviando-uma-requisição)
* [Verificação de certificado SSL / TLS do servidor](#verificação-de-certificado-ssl--tls-do-servidor)

## Aspectos gerais

Uma requisição HTTP é composta por:
* Método / verbo HTTP
* URL da requisição
* Versão do HTTP
* Cabeçalhos (opcionais)
* Corpo (opcional)
  
Uma resposta HTTP contém:
* Versão do HTTP
* Código de status HTTP
* Cabeçalhos (opcionais)
* Corpo (opcional)

## Criando

Para criar uma nova requisição, clique em uma coleção ou pasta e selecione "Nova requisição HTTP". Você também pode fazer isso clicando com o botão direito do mouse.

## Renomeando

Para renomear sua requisição, clique no ícone de lápis, no canto superior direito. Isso vai liberar para edição o nome da requisição.

## Copiando, colando e excluindo

Para copiar, colar ou excluir uma requisição, clique nela com o botão direito do mouse, no painel da esquerda.

Você pode selecionar mais de uma requisição ou pasta ao mesmo tempo e copiá-las ou excluí-las juntas.

![BotãoDireitoMouseRequisição](./imgs/right_click_request.png)

## Autenticação customizada

Autenticação customizada permite que você insira valores de autenticação, ao invés de digitar manualmente um header Authorization. Três tipos de autenticação são oferecidos: Basic, Bearer e Certificado de cliente.

### Autenticação Basic

Se autenticação Basic for usada, com um login "usr" e com uma senha "pwd", o seguinte header Authorization será adicionado na requisição enviada, de acordo com a [lógica de Basic Authentication](https://browse-tutorials.com/tools/basic-auth):

`Authorization: Basic dXNyOnB3ZA==`

### Autenticação Bearer

Se autenticação Bearer for usada, com um bearer token "my_token", então o seguinte header Authorization será adicionado na requisição enviada:

`Authorization: Bearer my_token`

### Autenticação por certificado de cliente

A autenticação por certificado de cliente difere dos métodos acima porque opera na camada de TLS, antes da requisição HTTP ser transmitida.

Os dois tipos aceitos de certificados de cliente são o PKCS#12 e o PEM. Esta [página](https://www.ryadel.com/en/ssl-certificates-standards-formats-extensions-cer-crt-key-pfx-pem-p7b-p7c-pfx-p12/?msclkid=ca7bc065ae0311ec98e66e2041811628) detalha alguns dos tipos de certificados que existem.

## Enviando uma requisição

Para enviar uma requisição, clique no botão "Enviar", no canto superior direito da tela. Você pode abortar a requisição clicando no botão "Cancelar". O tempo de timeout é de 5 minutos.

Você pode salvar o corpo da resposta em um arquivo, clicando no botão "Salvar como...".

![ExemploResposta](./imgs/response_save_as_example.png)

## Verificação de certificado SSL / TLS do servidor

Por padrão, o Pororoca realiza uma verificação de certificado SSL / TLS do servidor em conexões com HTTPS e, se houver uma falha na validação, a requisição não prosseguirá, como na figura abaixo.

![ExemploFalhaValidaçãoCertificadoTLS](./imgs/tls_certificate_validation_failure_example.png)

Para desabilitar a verificação de certificado TLS do servidor, clique no botão que fica na parte de baixo, ou vá ao menu superior, em "Opções", e selecione "Desabilitar verificação de TLS".

![DesabilitarVerificaçãoCertificadoTLS](./imgs/disable_tls_certificate_check.png)