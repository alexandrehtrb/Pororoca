using Pororoca.Desktop.Localization;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.TranslateRequest.WebSockets.ClientMessage;
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

    public PororocaWebSocketMessageType Type =>
        this.wsMsg.MessageType;

    public byte[]? Bytes
    {
        get
        {
            if (IsFromServer)
            {
                return ((PororocaWebSocketServerMessage)this.wsMsg).Bytes;
            }
            else if (IsClientMessageThatCanBeSaved)
            {
                var ms = (MemoryStream)((PororocaWebSocketClientMessageToSend)this.wsMsg).BytesStream;
                return ms.ToArray();
            }
            else
            {
                return null;
            }
        }
    }

    private bool IsClientMessageThatCanBeSaved =>
        this.wsMsg is PororocaWebSocketClientMessageToSend wsCliMsg
        && wsCliMsg.BytesStream is MemoryStream;

    public bool CanBeSavedToFile =>
        IsFromServer || IsClientMessageThatCanBeSaved;

    private readonly PororocaWebSocketMessage wsMsg;

    internal WebSocketExchangedMessageViewModel(PororocaWebSocketMessage wsMsg)
    {
        this.wsMsg = wsMsg;
        SetupTexts();
        // this language UI auto-update is disabled because ListBox items,
        // for some reason, do not change UI when texts are updated
        //Localizer.Instance.SubscribeToLanguageChange(SetupTexts);
    }

    private void SetupTexts()
    {
        if (this.wsMsg is PororocaWebSocketServerMessage wsSrvMsg)
            SetupTexts(wsSrvMsg.Direction, wsSrvMsg.MessageType, wsSrvMsg.Bytes.Length, (DateTimeOffset)wsSrvMsg.ReceivedAtUtc!, wsSrvMsg.Text);
        else if (this.wsMsg is PororocaWebSocketClientMessageToSend wsCliMsg)
            SetupTexts(wsCliMsg.Direction, wsCliMsg.MessageType, wsCliMsg.BytesLength, (DateTimeOffset)wsCliMsg.SentAtUtc!, wsCliMsg.Text);
    }

    private void SetupTexts(PororocaWebSocketMessageDirection direction,
                            PororocaWebSocketMessageType msgType,
                            long lengthInBytes,
                            DateTimeOffset dtUtc,
                            string? txtContent)
    {
        const string shortInstanceDtFormat = "yyyyMMdd-HHmmss";
        string instantDateTimeFormat = Localizer.Instance.WebSocketExchangedMessages.InstantDescriptionFormat;
        if (direction == PororocaWebSocketMessageDirection.FromClient)
        {
            IsFromClient = true;
            OriginDescription = Localizer.Instance.WebSocketExchangedMessages.FromClientToServer;
            InstantDescription = dtUtc.ToString(instantDateTimeFormat);
            ShortInstantDescription = dtUtc.LocalDateTime.ToString(shortInstanceDtFormat);
        }
        else if (direction == PororocaWebSocketMessageDirection.FromServer)
        {
            IsFromServer = true;
            OriginDescription = Localizer.Instance.WebSocketExchangedMessages.FromServerToClient;
            InstantDescription = dtUtc.ToString(instantDateTimeFormat);
            ShortInstantDescription = dtUtc.LocalDateTime.ToString(shortInstanceDtFormat);
        }

        string format;
        if (msgType == PororocaWebSocketMessageType.Close)
        {
            format = Localizer.Instance.WebSocketExchangedMessages.ClosingMessageContentDescriptionFormat;
            TypeDescription = Localizer.Instance.WebSocketClientMessage.MessageTypeClose;
        }
        else if (msgType == PororocaWebSocketMessageType.Binary)
        {
            format = Localizer.Instance.WebSocketExchangedMessages.BinaryContentDescriptionFormat;
            TypeDescription = Localizer.Instance.WebSocketClientMessage.MessageTypeBinary;
        }
        else
        {
            format = Localizer.Instance.WebSocketExchangedMessages.TextContentDescriptionFormat;
            TypeDescription = Localizer.Instance.WebSocketClientMessage.MessageTypeText;
        }

        MessageSizeDescription = string.Format(format, lengthInBytes);

        TextContent = txtContent ?? MessageSizeDescription;

        if (txtContent is not null)
        {
            IsJsonTextContent = JsonUtils.IsValidJson(TextContent);
        }
    }
}