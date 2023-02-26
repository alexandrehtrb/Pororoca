using System.Text.Json.Serialization;

namespace Pororoca.Domain.Features.Entities.Pororoca.WebSockets;

public enum PororocaWebSocketClientMessageContentMode
{
    Raw,
    File
}

public class PororocaWebSocketClientMessage : PororocaWebSocketMessage, ICloneable
{
    [JsonInclude]
    public Guid Id { get; set; }

    [JsonInclude]
    public string Name { get; set; }

    [JsonInclude]
    public PororocaWebSocketClientMessageContentMode ContentMode { get; set; }

    [JsonInclude]
    public string? RawContent { get; set; }

    [JsonInclude]
    public PororocaWebSocketMessageRawContentSyntax? RawContentSyntax { get; set; }

    [JsonInclude]
    public string? FileSrcPath { get; set; }

    [JsonInclude]
    public bool DisableCompressionForThis { get; set; }

    public PororocaWebSocketClientMessage(PororocaWebSocketMessageType msgType,
                                          string name,
                                          PororocaWebSocketClientMessageContentMode contentMode,
                                          string? rawContent,
                                          PororocaWebSocketMessageRawContentSyntax? rawContentSyntax,
                                          string? fileSrcPath,
                                          bool disableCompressionForThis) :
        base(PororocaWebSocketMessageDirection.FromClient, msgType)
    {
        Id = Guid.NewGuid();
        Name = name;
        ContentMode = contentMode;
        RawContent = rawContent;
        RawContentSyntax = rawContentSyntax;
        FileSrcPath = fileSrcPath;
        DisableCompressionForThis = disableCompressionForThis;
    }

#nullable disable warnings
    public PororocaWebSocketClientMessage()
    {
        // Parameterless constructor for JSON deserialization
    }
#nullable restore warnings

    public PororocaWebSocketClientMessage ClonePreservingId()
    {
        PororocaWebSocketClientMessage it = (PororocaWebSocketClientMessage)Clone();
        it.Id = Id;
        return it;
    }

    public object Clone() =>
        new PororocaWebSocketClientMessage(MessageType, Name, ContentMode, RawContent, RawContentSyntax, FileSrcPath, DisableCompressionForThis);
}