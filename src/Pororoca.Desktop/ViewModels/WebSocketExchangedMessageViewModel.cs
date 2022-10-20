using Pororoca.Desktop.Localization;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.TranslateRequest.WebSockets.ClientMessage;
using ReactiveUI;

namespace Pororoca.Desktop.ViewModels;

public sealed class WebSocketExchangedMessageViewModel : ViewModelBase
{
    private bool isFromServerField;
    public bool IsFromServer
    {
        get => this.isFromServerField;
        set => this.RaiseAndSetIfChanged(ref this.isFromServerField, value);
    }

    private bool isFromClientField;
    public bool IsFromClient
    {
        get => this.isFromClientField;
        set => this.RaiseAndSetIfChanged(ref this.isFromClientField, value);
    }

    private string? originDescriptionField;
    public string? OriginDescription
    {
        get => this.originDescriptionField;
        set => this.RaiseAndSetIfChanged(ref this.originDescriptionField, value);
    }

    private string? messageSizeDescriptionField;
    public string? MessageSizeDescription
    {
        get => this.messageSizeDescriptionField;
        set => this.RaiseAndSetIfChanged(ref this.messageSizeDescriptionField, value);
    }

    private string? instantDescriptionField;
    public string? InstantDescription
    {
        get => this.instantDescriptionField;
        set => this.RaiseAndSetIfChanged(ref this.instantDescriptionField, value);
    }

    private string? shortInstantDescriptionField;
    public string? ShortInstantDescription
    {
        get => this.shortInstantDescriptionField;
        set => this.RaiseAndSetIfChanged(ref this.shortInstantDescriptionField, value);
    }

    private string? typeDescriptionField;
    public string? TypeDescription
    {
        get => this.typeDescriptionField;
        set => this.RaiseAndSetIfChanged(ref this.typeDescriptionField, value);
    }

    private string? textContentField;
    public string? TextContent
    {
        get => this.textContentField;
        set => this.RaiseAndSetIfChanged(ref this.textContentField, value);
    }

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
                var ms = (MemoryStream) ((PororocaWebSocketClientMessageToSend) this.wsMsg).BytesStream;
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
            SetupTexts(wsSrvMsg.Direction, wsSrvMsg.MessageType, wsSrvMsg.Bytes.Length, (DateTimeOffset) wsSrvMsg.ReceivedAtUtc!, wsSrvMsg.Text);
        else if (this.wsMsg is PororocaWebSocketClientMessageToSend wsCliMsg)
            SetupTexts(wsCliMsg.Direction, wsCliMsg.MessageType, wsCliMsg.BytesLength, (DateTimeOffset) wsCliMsg.SentAtUtc!, wsCliMsg.Text);
    }

    private void SetupTexts(PororocaWebSocketMessageDirection direction,
                            PororocaWebSocketMessageType msgType,
                            long lengthInBytes,
                            DateTimeOffset dtUtc,
                            string? txtContent)
    {
        const string shortInstanceDtFormat = "yyyyMMdd-HHmmss";
        string instantDateTimeFormat = Localizer.Instance["WebSocketExchangedMessages/InstantDescriptionFormat"];
        if (direction == PororocaWebSocketMessageDirection.FromClient)
        {
            this.isFromClientField = true;
            this.originDescriptionField = Localizer.Instance["WebSocketExchangedMessages/FromClientToServer"];
            this.instantDescriptionField = dtUtc.ToString(instantDateTimeFormat);
            this.shortInstantDescriptionField = dtUtc.LocalDateTime.ToString(shortInstanceDtFormat);
        }
        else if (direction == PororocaWebSocketMessageDirection.FromServer)
        {
            this.isFromServerField = true;
            this.originDescriptionField = Localizer.Instance["WebSocketExchangedMessages/FromServerToClient"];
            this.instantDescriptionField = dtUtc.ToString(instantDateTimeFormat);
            this.shortInstantDescriptionField = dtUtc.LocalDateTime.ToString(shortInstanceDtFormat);
        }

        string format;
        if (msgType == PororocaWebSocketMessageType.Close)
        {
            format = Localizer.Instance["WebSocketExchangedMessages/ClosingMessageContentDescriptionFormat"];
            this.typeDescriptionField = Localizer.Instance["WebSocketClientMessage/MessageTypeClose"];
        }
        else if (msgType == PororocaWebSocketMessageType.Binary)
        {
            format = Localizer.Instance["WebSocketExchangedMessages/BinaryContentDescriptionFormat"];
            this.typeDescriptionField = Localizer.Instance["WebSocketClientMessage/MessageTypeBinary"];
        }
        else
        {
            format = Localizer.Instance["WebSocketExchangedMessages/TextContentDescriptionFormat"];
            this.typeDescriptionField = Localizer.Instance["WebSocketClientMessage/MessageTypeText"];
        }

        this.messageSizeDescriptionField = string.Format(format, lengthInBytes);

        this.textContentField = txtContent ?? this.messageSizeDescriptionField;
    }
}