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
    public Guid Id { get; init; }

    [JsonInclude]
    public string Name { get; set; }

    [JsonInclude]
    public PororocaWebSocketClientMessageContentMode ContentMode { get; set; }

    [JsonInclude]
    public string? RawContent { get; set; }

    [JsonInclude]
    public string? FileSrcPath { get; set; }

    [JsonInclude]
    public bool DisableCompressionForThis { get; set; }

    public PororocaWebSocketClientMessage(PororocaWebSocketMessageType msgType,
                                          string name,
                                          PororocaWebSocketClientMessageContentMode contentMode,
                                          string? rawContent,
                                          string? fileSrcPath,
                                          bool disableCompressionForThis) :
        base(PororocaWebSocketMessageDirection.FromClient, msgType)
    {
        Id = Guid.NewGuid();
        Name = name;
        ContentMode = contentMode;
        RawContent = rawContent;
        FileSrcPath = fileSrcPath;
        DisableCompressionForThis = disableCompressionForThis;
    }

#nullable disable warnings
    public PororocaWebSocketClientMessage()
    {
        // Parameterless constructor for JSON deserialization
    }
#nullable restore warnings

    public object Clone() =>
        new PororocaWebSocketClientMessage(MessageType, Name, ContentMode, RawContent, FileSrcPath, DisableCompressionForThis);
}