using Moq;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.TranslateRequest;
using Pororoca.Domain.Features.VariableResolution;
using Xunit;
using static Pororoca.Domain.Features.TranslateRequest.Common.PororocaRequestCommonValidator;
using static Pororoca.Domain.Features.TranslateRequest.WebSockets.ClientMessage.PororocaWebSocketClientMessageValidator;


namespace Pororoca.Domain.Tests.Features.TranslateRequest.WebSockets.ClientMessage;

public static class PororocaWebSocketClientMessageValidatorTests
{
    #region MOCKERS

    private static Mock<IPororocaVariableResolver> MockVariableResolver(Dictionary<string, string> kvs)
    {
        Mock<IPororocaVariableResolver> mockedVariableResolver = new();

        string f(string? k) => k == null ? string.Empty : kvs.TryGetValue(k, out string? value) ? value! : k;

        mockedVariableResolver.Setup(x => x.ReplaceTemplates(It.IsAny<string?>()))
                              .Returns((Func<string?, string>)f);

        return mockedVariableResolver;
    }

    private static FileExistsVerifier MockFileExistsVerifier(bool exists) =>
        (string s) => exists;

    private static FileExistsVerifier MockFileExistsVerifier(Dictionary<string, bool> fileList) =>
        (string s) =>
        {
            bool exists = fileList.ContainsKey(s) && fileList[s];
            return exists;
        };

    #endregion


    [Fact]
    public static void Should_allow_any_client_message_with_raw_content()
    {
        // GIVEN
        var mockedVariableResolver = MockVariableResolver(new());
        var fileExistsVerifier = MockFileExistsVerifier(false);
        PororocaWebSocketClientMessage msg = new(PororocaWebSocketMessageType.Text, string.Empty, PororocaWebSocketClientMessageContentMode.Raw, "msg content", null, null, false);

        // WHEN
        bool valid = IsValidClientMessage(mockedVariableResolver.Object, msg, fileExistsVerifier, out string? errorCode);

        // THEN
        Assert.True(valid);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates(It.IsAny<string>()), Times.Never);
        Assert.Null(errorCode);
    }

    [Fact]
    public static void Should_reject_client_message_with_file_content_if_resolved_file_is_not_found()
    {
        // GIVEN
        var mockedVariableResolver = MockVariableResolver(new()
        {
            { "{{FilePath}}", "./file.txt" }
        });
        var fileExistsVerifier = MockFileExistsVerifier(new Dictionary<string, bool>()
        {
            { "./file.txt", false }
        });
        PororocaWebSocketClientMessage msg = new(PororocaWebSocketMessageType.Text, string.Empty, PororocaWebSocketClientMessageContentMode.File, null, null, "{{FilePath}}", false);

        // WHEN
        bool valid = IsValidClientMessage(mockedVariableResolver.Object, msg, fileExistsVerifier, out string? errorCode);

        // THEN
        Assert.False(valid);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{FilePath}}"), Times.Once);
        Assert.Equal(TranslateRequestErrors.WebSocketClientMessageContentFileNotFound, errorCode);
    }

    [Fact]
    public static void Should_allow_client_message_with_file_content_if_resolved_file_is_found()
    {
        // GIVEN
        var mockedVariableResolver = MockVariableResolver(new()
        {
            { "{{FilePath}}", "./file.txt" }
        });
        var fileExistsVerifier = MockFileExistsVerifier(new Dictionary<string, bool>()
        {
            { "./file.txt", true }
        });
        PororocaWebSocketClientMessage msg = new(PororocaWebSocketMessageType.Text, string.Empty, PororocaWebSocketClientMessageContentMode.File, null, null, "{{FilePath}}", false);

        // WHEN
        bool valid = IsValidClientMessage(mockedVariableResolver.Object, msg, fileExistsVerifier, out string? errorCode);

        // THEN
        Assert.True(valid);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{FilePath}}"), Times.Once);
        Assert.Null(errorCode);
    }
}