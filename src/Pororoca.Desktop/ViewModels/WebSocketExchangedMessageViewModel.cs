using Pororoca.Desktop.Localization;
using Pororoca.Domain.Features.Common;
using Pororoca.Infrastructure.Features.WebSockets;
using ReactiveUI.Fody.Helpers;

namespace Pororoca.Desktop.ViewModels;

public sealed class WebSocketExchangedMessageViewModel : ViewModelBase
{
    [Reactive]
    public bool IsFromServer { get; set; }

    [Reactive]
    public bool IsFromClient { get; set; }

    [Reactive]
    public string? OriginDescription { get; set; }

    [Reactive]
    public string? MessageSizeDescription { get; set; }

    [Reactive]
    public string? InstantDescription { get; set; }

    [Reactive]
    public string? ShortInstantDescription { get; set; }

    [Reactive]
    public string? TypeDescription { get; set; }

    [Reactive]
    public string? TextContent { get; set; }

    [Reactive]
    public bool IsJsonTextContent { get; set; }

    public byte[]? Bytes
    {
        get
        {
            if (CanBeSavedToFile)
            {
                return ((MemoryStream)this.wsMsg.BytesStream).ToArray();
            }
            else
            {
                return null;
            }
        }
    }

    public bool CanBeSavedToFile =>
        this.wsMsg.BytesStream is MemoryStream;

    private readonly WebSocketMessage wsMsg;
    private readonly DateTimeOffset exchangedAt;

    internal WebSocketExchangedMessageViewModel(WebSocketMessage wsMsg, DateTimeOffset exchangedAt)
    {
        this.wsMsg = wsMsg;
        this.exchangedAt = exchangedAt;
        SetupTexts();
        // this language UI auto-update is disabled because ListBox items,
        // for some reason, do not change UI when texts are updated
        //Localizer.Instance.SubscribeToLanguageChange(SetupTexts);
    }

    private void SetupTexts()
    {
        const string shortInstanceDtFormat = "yyyyMMdd-HHmmss";
        string instantDateTimeFormat = "HH:mm:ss, yyyy-MM-dd, 'GMT'z'h'";
        if (this.wsMsg.Direction == WebSocketMessageDirection.FromClient)
        {
            IsFromClient = true;
            OriginDescription = Localizer.Instance.WebSocketExchangedMessages.FromClientToServer;
        }
        else if (this.wsMsg.Direction == WebSocketMessageDirection.FromServer)
        {
            IsFromServer = true;
            OriginDescription = Localizer.Instance.WebSocketExchangedMessages.FromServerToClient;
        }
        InstantDescription = this.exchangedAt.ToString(instantDateTimeFormat);
        ShortInstantDescription = this.exchangedAt.LocalDateTime.ToString(shortInstanceDtFormat);

        string format;
        if (this.wsMsg.Type == System.Net.WebSockets.WebSocketMessageType.Close)
        {
            format = Localizer.Instance.WebSocketExchangedMessages.ClosingMessageContentDescriptionFormat;
            TypeDescription = Localizer.Instance.WebSocketClientMessage.MessageTypeClose;
        }
        else if (this.wsMsg.Type == System.Net.WebSockets.WebSocketMessageType.Binary)
        {
            format = Localizer.Instance.WebSocketExchangedMessages.BinaryContentDescriptionFormat;
            TypeDescription = Localizer.Instance.WebSocketClientMessage.MessageTypeBinary;
        }
        else
        {
            format = Localizer.Instance.WebSocketExchangedMessages.TextContentDescriptionFormat;
            TypeDescription = Localizer.Instance.WebSocketClientMessage.MessageTypeText;
        }

        MessageSizeDescription = string.Format(format, this.wsMsg.BytesLength);

        string? txtContent = (this.wsMsg.Type == System.Net.WebSockets.WebSocketMessageType.Text
                           || this.wsMsg.Type == System.Net.WebSockets.WebSocketMessageType.Close) ?
                             this.wsMsg.ReadAsUtf8Text() : null;

        if (txtContent is null)
        {
            IsJsonTextContent = false;
            TextContent = MessageSizeDescription;
        }
        else
        {
            IsJsonTextContent = JsonUtils.IsValidJson(txtContent);
            TextContent = IsJsonTextContent ?
                          JsonUtils.PrettifyJson(txtContent) :
                          txtContent;
        }
    }
}