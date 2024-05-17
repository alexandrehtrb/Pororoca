namespace Pororoca.Domain.Features.Entities.Pororoca.WebSockets;

public enum PororocaWebSocketClientMessageContentMode
{
    Raw,
    File
}

public record PororocaWebSocketClientMessage
(
    PororocaWebSocketMessageType MessageType,
    string Name,
    PororocaWebSocketClientMessageContentMode ContentMode = PororocaWebSocketClientMessageContentMode.Raw,
    string? RawContent = null,
    PororocaWebSocketMessageRawContentSyntax? RawContentSyntax = null,
    string? FileSrcPath = null,
    bool DisableCompressionForThis = false
) : PororocaWebSocketMessage(PororocaWebSocketMessageDirection.FromClient, MessageType)
{

#nullable disable warnings
    // Parameterless constructor for JSON deserialization
    public PororocaWebSocketClientMessage() : this(PororocaWebSocketMessageType.Text, string.Empty) { }
#nullable restore warnings

    public PororocaWebSocketClientMessage Copy() => this with { };
}