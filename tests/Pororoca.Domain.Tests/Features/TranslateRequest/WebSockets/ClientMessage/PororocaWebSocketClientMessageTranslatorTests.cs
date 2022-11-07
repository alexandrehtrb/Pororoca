using System.Text;
using Moq;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.TranslateRequest;
using Pororoca.Domain.Features.VariableResolution;
using Xunit;
using static Pororoca.Domain.Features.TranslateRequest.Common.PororocaRequestCommonValidator;
using static Pororoca.Domain.Features.TranslateRequest.WebSockets.ClientMessage.PororocaWebSocketClientMessageTranslator;


namespace Pororoca.Domain.Tests.Features.TranslateRequest.WebSockets.ClientMessage;

public static class PororocaWebSocketClientMessageTranslatorTests
{
    #region MOCKERS

    private static Mock<IPororocaVariableResolver> MockVariableResolver(Dictionary<string, string> kvs)
    {
        Mock<IPororocaVariableResolver> mockedVariableResolver = new();

        string f(string? k) => k == null ? string.Empty : kvs.ContainsKey(k) ? kvs[k] : k;

        mockedVariableResolver.Setup(x => x.ReplaceTemplates(It.IsAny<string?>()))
                              .Returns((Func<string?, string>)f);

        return mockedVariableResolver;
    }

    #endregion

    [Theory]
    [InlineData(PororocaWebSocketMessageType.Text, true)]
    [InlineData(PororocaWebSocketMessageType.Text, false)]
    [InlineData(PororocaWebSocketMessageType.Binary, true)]
    [InlineData(PororocaWebSocketMessageType.Binary, false)]
    [InlineData(PororocaWebSocketMessageType.Close, true)]
    [InlineData(PororocaWebSocketMessageType.Close, false)]
    public static void Should_correctly_translate_client_message_with_resolved_raw_content(PororocaWebSocketMessageType msgType, bool disableCompressionForMsg)
    {
        // GIVEN
        var mockedVariableResolver = MockVariableResolver(new()
        {
            { "Hello {{Name}}", "Hello brother" }
        });
        PororocaWebSocketClientMessage msg = new(msgType, string.Empty, PororocaWebSocketClientMessageContentMode.Raw, "Hello {{Name}}", null, null, disableCompressionForMsg);

        // WHEN
        bool valid = TryTranslateClientMessage(mockedVariableResolver.Object, msg, out var resolvedMsg, out string? errorCode);

        // THEN
        Assert.True(valid);
        Assert.Null(errorCode);
        
        Assert.NotNull(resolvedMsg);
        Assert.Equal(msgType, resolvedMsg!.MessageType);
        Assert.Equal(disableCompressionForMsg, resolvedMsg!.DisableCompressionForThis);
        Assert.Equal("Hello brother", resolvedMsg!.Text);
        Assert.IsAssignableFrom<MemoryStream>(resolvedMsg!.BytesStream);
        Assert.Equal(13, resolvedMsg!.BytesLength);
        Assert.Equal("Hello brother", Encoding.UTF8.GetString(((MemoryStream)resolvedMsg!.BytesStream).ToArray()));

        mockedVariableResolver.Verify(x => x.ReplaceTemplates("Hello {{Name}}"), Times.Once);
    }

    [Theory]
    [InlineData(PororocaWebSocketMessageType.Text, true)]
    [InlineData(PororocaWebSocketMessageType.Text, false)]
    [InlineData(PororocaWebSocketMessageType.Binary, true)]
    [InlineData(PororocaWebSocketMessageType.Binary, false)]
    [InlineData(PororocaWebSocketMessageType.Close, true)]
    [InlineData(PororocaWebSocketMessageType.Close, false)]
    public static void Should_correctly_translate_client_message_with_resolved_file_content(PororocaWebSocketMessageType msgType, bool disableCompressionForMsg)
    {
        // GIVEN
        var mockedVariableResolver = MockVariableResolver(new()
        {
            { "{{FilePath}}", GetTestFilePath("testfilecontent1.json") }
        });
        PororocaWebSocketClientMessage msg = new(msgType, string.Empty, PororocaWebSocketClientMessageContentMode.File, null, null, "{{FilePath}}", disableCompressionForMsg);

        // WHEN
        bool valid = TryTranslateClientMessage(mockedVariableResolver.Object, msg, out var resolvedMsg, out string? errorCode);

        // THEN
        Assert.True(valid);
        Assert.Null(errorCode);
        
        Assert.NotNull(resolvedMsg);
        Assert.Equal(msgType, resolvedMsg!.MessageType);
        Assert.Equal(disableCompressionForMsg, resolvedMsg!.DisableCompressionForThis);
        Assert.Null(resolvedMsg!.Text);
        Assert.IsAssignableFrom<FileStream>(resolvedMsg!.BytesStream);
        Assert.True(resolvedMsg!.BytesStream.CanRead);
        Assert.False(resolvedMsg!.BytesStream.CanWrite);
        Assert.Equal(8, resolvedMsg!.BytesLength);
        Assert.Equal("{\"id\":1}", new StreamReader(resolvedMsg!.BytesStream).ReadToEnd());

        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{FilePath}}"), Times.Once);
    }

    [Theory]
    [InlineData(PororocaWebSocketMessageType.Text, true)]
    [InlineData(PororocaWebSocketMessageType.Text, false)]
    [InlineData(PororocaWebSocketMessageType.Binary, true)]
    [InlineData(PororocaWebSocketMessageType.Binary, false)]
    [InlineData(PororocaWebSocketMessageType.Close, true)]
    [InlineData(PororocaWebSocketMessageType.Close, false)]
    public static void Should_return_error_in_case_of_exception(PororocaWebSocketMessageType msgType, bool disableCompressionForMsg)
    {
        // GIVEN
        var mockedVariableResolver = MockVariableResolver(new()
        {
            { "{{FilePath}}", GetTestFilePath("invalid_file.aaa") } // file that does not exist
        });
        PororocaWebSocketClientMessage msg = new(msgType, string.Empty, PororocaWebSocketClientMessageContentMode.File, null, null, "{{FilePath}}", disableCompressionForMsg);

        // WHEN
        bool valid = TryTranslateClientMessage(mockedVariableResolver.Object, msg, out var resolvedMsg, out string? errorCode);

        // THEN
        Assert.False(valid);
        Assert.Equal(TranslateRequestErrors.WebSocketUnknownClientMessageTranslationError, errorCode);        
        Assert.Null(resolvedMsg);

        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{FilePath}}"), Times.Once);
    }

    private static string GetTestFilePath(string fileName)
    {
        var testDataDirInfo = new DirectoryInfo(Environment.CurrentDirectory).Parent!.Parent!.Parent!;
        return Path.Combine(testDataDirInfo.FullName, "TestData", fileName);
    }
}