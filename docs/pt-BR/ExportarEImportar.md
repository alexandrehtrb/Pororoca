# Exportar e Importar

Coleções e ambientes podem ser exportados e importados para / de arquivos, que podem ser salvos e compartilhados com outras pessoas.

Pororoca oferece compatibilidade total de exportação e importação com o Postman.

## Exportar

Para exportar uma coleção ou um ambiente, clique neles no painel da esquerda, depois clique no botão "Exportar coleção..." ou "Exportar ambiente...", respectivamente.

A opção "Incluir variáveis secretas", se selecionada, incluirá os valores de variáveis secretas nos arquivos exportados. Se não estiver selecionada, os valores das variáveis secretas serão substituídos por um texto vazio.

O formato do arquivo de destino pode ser escolhido na janela de exportação.

Ao exportar uma coleção em formato Pororoca, os ambientes da coleção também serão transportados no arquivo - não há necessidade de exportar cada ambiente individualmente.

![ExportarColeçãoFormatoArquivo](./imgs/export_collection_format.png)

## Importar

### Coleção

Para importar uma coleção, vá ao menu superior, selecione "Arquivo", depois "Importar coleção...". Uma janela de seleção de arquivo se abrirá.

*Nota*: Requisições do Postman que usam parâmetros arquivo usam um esquema diferente de caminho de arquivos do que o Pororoca usa. Após a importação, confira se os caminhos de arquivos estão corretos, por exemplo, nos corpos das requisições.

### Ambiente

Para importar um ambiente, expanda sua coleção no painel esquerdo e clique com o botão direito do mouse em "Ambientes". Depois, selecione "Importar ambiente...". Uma janela de seleção de arquivo se abrirá.