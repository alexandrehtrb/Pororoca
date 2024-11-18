using System.Text;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.TranslateRequest;
using Pororoca.Domain.Features.VariableResolution;
using Xunit;
using static Pororoca.Domain.Features.TranslateRequest.WebSockets.ClientMessage.PororocaWebSocketClientMessageTranslator;


namespace Pororoca.Domain.Tests.Features.TranslateRequest.WebSockets.ClientMessage;

public static class PororocaWebSocketClientMessageTranslatorTests
{
    #region MOCKERS

    private static IPororocaVariableResolver MockVariableResolver(Dictionary<string, string> kvs)
    {
        PororocaCollection col = new("col");
        col.Variables.AddRange(kvs.Select(kv => new PororocaVariable(true, kv.Key, kv.Value, false)));
        return col;
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
        var varResolver = MockVariableResolver(new()
        {
            { "Name", "brother" }
        });
        PororocaWebSocketClientMessage msg = new(msgType, string.Empty, PororocaWebSocketClientMessageContentMode.Raw, "Hello {{Name}}", null, null, disableCompressionForMsg);

        // WHEN
        bool valid = TryTranslateClientMessage(varResolver.GetEffectiveVariables(), msg, out var resolvedStream, out string? errorCode);

        // THEN
        Assert.True(valid);
        Assert.Null(errorCode);

        Assert.NotNull(resolvedStream);
        var ms = Assert.IsAssignableFrom<MemoryStream>(resolvedStream!);
        Assert.Equal(13, ms.Length);
        Assert.Equal("Hello brother", Encoding.UTF8.GetString(ms.ToArray()));
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
        var varResolver = MockVariableResolver(new()
        {
            { "FilePath", GetTestFilePath("testfilecontent1.json") }
        });
        PororocaWebSocketClientMessage msg = new(msgType, string.Empty, PororocaWebSocketClientMessageContentMode.File, null, null, "{{FilePath}}", disableCompressionForMsg);

        // WHEN
        bool valid = TryTranslateClientMessage(varResolver.GetEffectiveVariables(), msg, out var resolvedStream, out string? errorCode);

        // THEN
        Assert.True(valid);
        Assert.Null(errorCode);

        Assert.NotNull(resolvedStream);
        var fs = Assert.IsAssignableFrom<FileStream>(resolvedStream!);
        Assert.Equal(8, fs.Length);
        Assert.Equal("{\"id\":1}", new StreamReader(fs).ReadToEnd());
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
        var varResolver = MockVariableResolver(new()
        {
            { "FilePath", GetTestFilePath("invalid_file.aaa") } // file that does not exist
        });
        PororocaWebSocketClientMessage msg = new(msgType, string.Empty, PororocaWebSocketClientMessageContentMode.File, null, null, "{{FilePath}}", disableCompressionForMsg);

        // WHEN
        bool valid = TryTranslateClientMessage(varResolver.GetEffectiveVariables(), msg, out var resolvedStream, out string? errorCode);

        // THEN
        Assert.False(valid);
        Assert.Equal(TranslateRequestErrors.WebSocketUnknownClientMessageTranslationError, errorCode);
        Assert.Null(resolvedStream);
    }
}