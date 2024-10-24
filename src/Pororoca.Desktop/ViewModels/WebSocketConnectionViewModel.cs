using System.Collections.ObjectModel;
using System.Net;
using System.Reactive;
using System.Security.Authentication;
using System.Threading.Channels;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using Pororoca.Desktop.Converters;
using Pororoca.Desktop.ExportImport;
using Pororoca.Desktop.HotKeys;
using Pororoca.Desktop.Localization;
using Pororoca.Desktop.ViewModels.DataGrids;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.Requester;
using Pororoca.Domain.Features.TranslateRequest;
using Pororoca.Domain.Features.VariableResolution;
using Pororoca.Infrastructure.Features.Requester;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using static Pororoca.Desktop.Localization.TimeTextFormatter;
using static Pororoca.Domain.Features.Common.AvailablePororocaRequestSelectionOptions;
using static Pororoca.Domain.Features.Common.HttpStatusCodeFormatter;
using static Pororoca.Domain.Features.Common.HttpVersionFormatter;
using static Pororoca.Domain.Features.TranslateRequest.WebSockets.ClientMessage.PororocaWebSocketClientMessageTranslator;
using static Pororoca.Domain.Features.TranslateRequest.WebSockets.ClientMessage.PororocaWebSocketClientMessageValidator;
using static Pororoca.Domain.Features.TranslateRequest.WebSockets.Connection.PororocaWebSocketConnectionTranslator;
using static Pororoca.Domain.Features.TranslateRequest.WebSockets.Connection.PororocaWebSocketConnectionValidator;

namespace Pororoca.Desktop.ViewModels;

public sealed class WebSocketConnectionViewModel : CollectionOrganizationItemParentViewModel<WebSocketClientMessageViewModel>
{
    #region COLLECTION ORGANIZATION

    public ReactiveCommand<Unit, Unit> AddNewWebSocketClientMessageCmd { get; }

    #endregion

    #region CONNECTION

    private readonly CollectionViewModel col;
    private readonly IPororocaHttpClientProvider httpClientProvider;
    private readonly PororocaWebSocketConnector connector;

    [Reactive]
    public string ConnectDisconnectCancelButtonText { get; set; }

    [Reactive]
    public string ConnectDisconnectCancelButtonToolTip { get; set; }

    private PororocaWebSocketConnectorState connectionStateField;
    public PororocaWebSocketConnectorState ConnectionState
    {
        get => this.connectionStateField;
        set
        {
            NameEditableVm.Icon = value switch
            {
                PororocaWebSocketConnectorState.Connected or
                PororocaWebSocketConnectorState.Disconnecting => EditableTextBlockIcon.ConnectedWebSocket,
                _ => EditableTextBlockIcon.DisconnectedWebSocket
            };
            IsConnected = value switch
            {
                PororocaWebSocketConnectorState.Connected or
                PororocaWebSocketConnectorState.Disconnecting => true,
                _ => false
            };
            IsConnectingOrDisconnecting = value switch
            {
                PororocaWebSocketConnectorState.Connecting or
                PororocaWebSocketConnectorState.Disconnecting => true,
                _ => false
            };
            ConnectDisconnectCancelButtonText = value switch
            {
                PororocaWebSocketConnectorState.Disconnected => Localizer.Instance.WebSocketConnection.Connect,
                PororocaWebSocketConnectorState.Connecting => Localizer.Instance.WebSocketConnection.CancelConnect,
                PororocaWebSocketConnectorState.Connected => Localizer.Instance.WebSocketConnection.Disconnect,
                PororocaWebSocketConnectorState.Disconnecting => Localizer.Instance.WebSocketConnection.CancelDisconnect,
                _ => string.Empty
            };
            ConnectDisconnectCancelButtonToolTip = value switch
            {
                PororocaWebSocketConnectorState.Disconnected => Localizer.Instance.WebSocketConnection.ConnectToolTip,
                PororocaWebSocketConnectorState.Connecting => Localizer.Instance.WebSocketConnection.CancelConnectToolTip,
                PororocaWebSocketConnectorState.Connected => Localizer.Instance.WebSocketConnection.DisconnectToolTip,
                PororocaWebSocketConnectorState.Disconnecting => Localizer.Instance.WebSocketConnection.CancelDisconnect,
                _ => string.Empty
            };
            this.connectionStateField = value;
        }
    }

    [Reactive]
    public bool IsConnected { get; set; }

    [Reactive]
    public bool IsConnectingOrDisconnecting { get; set; }

    [Reactive]
    public bool IsDisableTlsVerificationVisible { get; set; }

    private CancellationTokenSource? cancelConnectionAttemptTokenSource;

    private CancellationTokenSource? cancelDisconnectionAttemptTokenSource;

    public ReactiveCommand<Unit, Unit> DisableTlsVerificationCmd { get; }

    // To preserve the state of the last shown request tab

    [Reactive]
    public int SelectedConnectionTabIndex { get; set; }

    private string urlField;
    public string Url
    {
        get => this.urlField;
        set
        {
            this.RaiseAndSetIfChanged(ref this.urlField, value);
            // clear invalid warnings if user starts typing to fix them
            if (HasUrlValidationProblem)
                ClearInvalidConnectionWarnings();
        }
    }

    [Reactive]
    public bool HasUrlValidationProblem { get; set; }

    [Reactive]
    public string ResolvedUrlToolTip { get; set; }

    public override ObservableCollection<WebSocketClientMessageViewModel> Items { get; } = new();

    #endregion

    #region CONNECTION REQUEST HTTP VERSION

    public ObservableCollection<string> HttpVersionSelectionOptions { get; }

    private int httpVersionSelectedIndexField;
    public int HttpVersionSelectedIndex
    {
        get => this.httpVersionSelectedIndexField;
        set
        {
            this.RaiseAndSetIfChanged(ref this.httpVersionSelectedIndexField, value);
            // clear invalid warnings if user starts typing to fix them
            if (HasHttpVersionValidationProblem)
                ClearInvalidConnectionWarnings();
        }
    }

    [Reactive]
    public bool HasHttpVersionValidationProblem { get; set; }

    private decimal HttpVersion =>
        AvailableHttpVersionsForWebSockets[HttpVersionSelectedIndex];

    #endregion

    #region REQUEST VALIDATION MESSAGE

    private string? invalidConnectionErrorCodeField;
    private string? InvalidConnectionErrorCode
    {
        get => this.invalidConnectionErrorCodeField;
        set
        {
            this.invalidConnectionErrorCodeField = value;
            IsInvalidConnectionErrorVisible = value is not null;
            InvalidConnectionError = value switch
            {
                TranslateRequestErrors.WindowsAuthLoginCannotBeBlank => Localizer.Instance.RequestValidation.WindowsAuthLoginCannotBeBlank,
                TranslateRequestErrors.WindowsAuthPasswordCannotBeBlank => Localizer.Instance.RequestValidation.WindowsAuthPasswordCannotBeBlank,
                TranslateRequestErrors.WindowsAuthDomainCannotBeBlank => Localizer.Instance.RequestValidation.WindowsAuthDomainCannotBeBlank,
                TranslateRequestErrors.ClientCertificatePkcs12CertificateFileNotFound => Localizer.Instance.RequestValidation.ClientCertificateFileNotFound,
                TranslateRequestErrors.ClientCertificatePemCertificateFileNotFound => Localizer.Instance.RequestValidation.ClientCertificateFileNotFound,
                TranslateRequestErrors.ClientCertificatePkcs12PasswordCannotBeBlank => Localizer.Instance.RequestValidation.ClientCertificatePkcs12PasswordCannotBeBlank,
                TranslateRequestErrors.ClientCertificatePemPrivateKeyFileNotFound => Localizer.Instance.RequestValidation.ClientCertificatePemPrivateKeyFileNotFound,
                TranslateRequestErrors.InvalidUrl => Localizer.Instance.RequestValidation.InvalidUrl,
                TranslateRequestErrors.Http2UnavailableInOSVersion => Localizer.Instance.RequestValidation.Http2Unavailable,
                TranslateRequestErrors.WebSocketHttpVersionUnavailable => Localizer.Instance.RequestValidation.WebSocketHttpVersionUnavailable,
                TranslateRequestErrors.WebSocketCompressionMaxWindowBitsOutOfRange => Localizer.Instance.RequestValidation.WebSocketCompressionMaxWindowBitsOutOfRange,
                TranslateRequestErrors.WebSocketUnknownConnectionTranslationError => Localizer.Instance.RequestValidation.WebSocketUnknownConnectionTranslationError,
                _ => Localizer.Instance.RequestValidation.WebSocketUnknownConnectionTranslationError
            };
            HasUrlValidationProblem = (value == TranslateRequestErrors.InvalidUrl);
            HasHttpVersionValidationProblem = (value == TranslateRequestErrors.Http2UnavailableInOSVersion);

            // do not colourize errors and switch tab if problem is in collection-scoped auth
            if (RequestAuthDataCtx.AuthMode == PororocaRequestAuthMode.InheritFromCollection)
                return;

            RequestAuthDataCtx.HighlightValidationProblems(value);

            if (RequestAuthDataCtx.HasValidationProblem)
            {
                // TODO: Improve this, do not use fixed values to resolve index
                SelectedConnectionTabIndex = 1;
            }
        }
    }

    [Reactive]
    public bool IsInvalidConnectionErrorVisible { get; set; }
    [Reactive]
    public string? InvalidConnectionError { get; set; }

    #endregion

    #region CONNECTION OPTIONS

    [Reactive]
    public int ConnectionOptionSelectedIndex { get; set; }

    #region CONNECTION OPTION HEADERS

    public RequestHeadersDataGridViewModel RequestHeadersTableVm { get; }

    #endregion

    #region CONNECTION OPTION SUBPROTOCOLS

    public KeyValueParamsDataGridViewModel SubprotocolsTableVm { get; }

    #endregion

    #region CONNECTION OPTION COMPRESSION

    [Reactive]
    public bool EnableCompression { get; set; }

    [Reactive]
    public bool CompressionClientContextTakeoverEnabled { get; set; }

    [Reactive]
    public int CompressionClientMaxWindowBits { get; set; }

    [Reactive]
    public bool CompressionServerContextTakeoverEnabled { get; set; }

    [Reactive]
    public int CompressionServerMaxWindowBits { get; set; }

    #endregion

    #endregion

    #region CONNECTION REQUEST AUTH

    [Reactive]
    public RequestAuthViewModel RequestAuthDataCtx { get; set; }

    #endregion

    #region CONNECTION RESPONSE

    [Reactive]
    public bool WasConnectionSuccessful { get; private set; }

    [Reactive]
    public string? ConnectionExceptionContent { get; set; }

    [Reactive]
    public string? ResponseStatusCodeElapsedTimeTitle { get; private set; }

    public KeyValueParamsDataGridViewModel ConnectionResponseHeadersTableVm { get; }

    #endregion

    #region EXCHANGED MESSAGES

    private string? invalidClientMessageErrorCodeField;
    private string? InvalidClientMessageErrorCode
    {
        get => this.invalidClientMessageErrorCodeField;
        set
        {
            this.invalidClientMessageErrorCodeField = value;
            IsInvalidClientMessageErrorVisible = value is not null;
            InvalidClientMessageError = value switch
            {
                TranslateRequestErrors.WebSocketNotConnected => Localizer.Instance.RequestValidation.WebSocketNotConnected,
                TranslateRequestErrors.WebSocketClientMessageContentFileNotFound => Localizer.Instance.RequestValidation.WebSocketClientMessageContentFileNotFound,
                TranslateRequestErrors.WebSocketUnknownClientMessageTranslationError => Localizer.Instance.RequestValidation.WebSocketUnknownClientMessageTranslationError,
                _ => Localizer.Instance.RequestValidation.WebSocketUnknownClientMessageTranslationError
            };
        }
    }

    [Reactive]
    public bool IsInvalidClientMessageErrorVisible { get; set; }
    [Reactive]
    public string? InvalidClientMessageError { get; set; }

    [Reactive]
    public bool IsSendingAMessage { get; set; }

    public ObservableCollection<WebSocketExchangedMessageViewModel> ExchangedMessages { get; }

    [Reactive]
    public int MessageToSendSelectedIndex { get; set; }

    public ReactiveCommand<Unit, Unit> SendMessageCmd { get; }

    #endregion

    #region MESSAGE DETAIL

    [Reactive]
    public string? SelectedExchangedMessageType { get; set; }

    [Reactive]
    public TextDocument? SelectedExchangedMessageContentTextDocument { get; set; }

    public string? SelectedExchangedMessageContent
    {
        get => SelectedExchangedMessageContentTextDocument?.Text;
        set => SelectedExchangedMessageContentTextDocument = new(value ?? string.Empty);
    }

    private WebSocketExchangedMessageViewModel? selectedExchangedMessageField;
    public WebSocketExchangedMessageViewModel? SelectedExchangedMessage
    {
        get => this.selectedExchangedMessageField;
        set
        {
            this.RaiseAndSetIfChanged(ref this.selectedExchangedMessageField, value);
            OnSelectedExchangedMessageChanged(value);
        }
    }

    public bool IsSelectedExchangedMessageContentJson { get; set; }

    [Reactive]
    public bool IsSaveSelectedExchangedMessageToFileVisible { get; set; }

    public ReactiveCommand<Unit, Unit> SaveSelectedExchangedMessageToFileCmd { get; }

    #endregion

    public WebSocketConnectionViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                                        CollectionViewModel col,
                                        PororocaWebSocketConnection ws) : base(parentVm, ws.Name)
    {
        #region COLLECTION ORGANIZATION
        Localizer.Instance.SubscribeToLanguageChange(OnLanguageChanged);

        NameEditableVm.Icon = EditableTextBlockIcon.DisconnectedWebSocket;
        AddNewWebSocketClientMessageCmd = ReactiveCommand.Create(AddNewWebSocketClientMessage);
        #endregion

        #region CONNECTION

        this.col = col;
        this.httpClientProvider = PororocaHttpClientProvider.Singleton;
        this.connector = new(OnWebSocketConnectionChanged, OnWebSocketMessageSending);

        if (ws.ClientMessages is not null)
        {
            foreach (var wsReqMsg in ws.ClientMessages)
                Items.Add(new(this, wsReqMsg));

            RefreshSubItemsAvailableMovements();
        }

        ConnectDisconnectCancelButtonText = Localizer.Instance.WebSocketConnection.Connect;
        ConnectDisconnectCancelButtonToolTip = Localizer.Instance.WebSocketConnection.ConnectToolTip;
        DisableTlsVerificationCmd = ReactiveCommand.Create(DisableTlsVerification);

        #endregion

        #region CONNECTION REQUEST HTTP VERSION AND URL
        ResolvedUrlToolTip = this.urlField = ws.Url;

        HttpVersionSelectionOptions = new(AvailableHttpVersionsForWebSockets.Select(FormatHttpVersion));
        int httpVersionSelectionIndex = HttpVersionSelectionOptions.IndexOf(FormatHttpVersion(ws.HttpVersion));
        HttpVersionSelectedIndex = httpVersionSelectionIndex >= 0 ? httpVersionSelectionIndex : 0;
        #endregion

        #region CONNECTION REQUEST AUTH
        RequestAuthDataCtx = new(ws.CustomAuth, true, ClearInvalidConnectionWarnings);
        #endregion

        #region CONNECTION OPTIONS

        ConnectionOptionSelectedIndex = 0;

        #region CONNECTION OPTION HEADERS

        RequestHeadersTableVm = new(ws.Headers);

        #endregion

        #region CONNECTION OPTION SUBPROTOCOLS

        SubprotocolsTableVm = new(ws.Subprotocols);

        #endregion

        #region CONNECTION OPTION COMPRESSION

        if (ws.EnableCompression)
        {
            EnableCompression = true;
            CompressionClientContextTakeoverEnabled = ws.CompressionOptions!.ClientContextTakeover;
            CompressionClientMaxWindowBits = ws.CompressionOptions!.ClientMaxWindowBits;
            CompressionServerContextTakeoverEnabled = ws.CompressionOptions!.ServerContextTakeover;
            CompressionServerMaxWindowBits = ws.CompressionOptions!.ServerMaxWindowBits;
        }
        else
        {
            EnableCompression = false;
            CompressionClientContextTakeoverEnabled = PororocaWebSocketCompressionOptions.DefaultClientContextTakeover;
            CompressionClientMaxWindowBits = PororocaWebSocketCompressionOptions.DefaultClientMaxWindowBits;
            CompressionServerContextTakeoverEnabled = PororocaWebSocketCompressionOptions.DefaultServerContextTakeover;
            CompressionServerMaxWindowBits = PororocaWebSocketCompressionOptions.DefaultServerMaxWindowBits;
        }

        #endregion

        #endregion

        #region EXCHANGED MESSAGES
        ConnectionResponseHeadersTableVm = new(new List<PororocaKeyValueParam>());
        ExchangedMessages = new();
        SendMessageCmd = ReactiveCommand.CreateFromTask(SendMessageAsync);
        #endregion

        #region MESSAGE DETAIL
        // we need to always set SelectedExchangedMessageContent with a value, even if it is null,
        // to initialize with a TextDocument object
        SelectedExchangedMessageContent = null;
        SaveSelectedExchangedMessageToFileCmd = ReactiveCommand.CreateFromTask(SaveSelectedExchangedMessageToFileAsync);
        #endregion
    }

    #region COLLECTION ORGANIZATION

    public override void PasteToThis()
    {
        var itemsToPaste = ClipboardArea.Instance.FetchCopiesOfWebSocketClientMessages();
        foreach (var itemToPaste in itemsToPaste)
        {
            AddWebSocketClientMessage(itemToPaste);
        }
    }

    private void AddNewWebSocketClientMessage()
    {
        PororocaWebSocketClientMessage wsReqMsg = new(PororocaWebSocketMessageType.Text,
                                                      Localizer.Instance.WebSocketClientMessage.NewMessage);
        AddWebSocketClientMessage(wsReqMsg, showItemInScreen: true);
    }

    public void AddWebSocketClientMessage(PororocaWebSocketClientMessage wsReqMsgToAdd, bool showItemInScreen = false)
    {
        WebSocketClientMessageViewModel wsReqMsgToAddVm = new(this, wsReqMsgToAdd);

        Items.Add(wsReqMsgToAddVm);
        IsExpanded = true;
        RefreshSubItemsAvailableMovements();
        SetAsItemInFocus(wsReqMsgToAddVm, showItemInScreen);
    }

    private void OnLanguageChanged()
    {
        if (InvalidConnectionErrorCode is not null)
        {
            string code = InvalidConnectionErrorCode;
            InvalidConnectionErrorCode = code; // this will trigger an update
        }

        if (InvalidClientMessageErrorCode is not null)
        {
            string code = InvalidClientMessageErrorCode;
            InvalidClientMessageErrorCode = code; // this will trigger an update
        }

        // this will trigger an update on the connect/disconnect/cancel ws button texts
        var state = this.connectionStateField;
        ConnectionState = state;
    }

    #endregion

    #region REQUEST HTTP METHOD, HTTP VERSION AND URL

    public void UpdateResolvedUrlToolTip() =>
        ResolvedUrlToolTip = IPororocaVariableResolver.ReplaceTemplates(Url, ((IPororocaVariableResolver)this.col).GetEffectiveVariables());

    #endregion

    #region CONNECTION REQUEST HEADERS

    #endregion

    #region CONNECTION OPTION SUBPROTOCOLS

    #endregion

    #region CONVERT VIEW INPUTS TO REQUEST ENTITY

    private PororocaWebSocketCompressionOptions? WrapCompressionOptionsFromInputs() =>
        EnableCompression ?
            new(CompressionClientMaxWindowBits,
                CompressionClientContextTakeoverEnabled,
                CompressionServerMaxWindowBits,
                CompressionServerContextTakeoverEnabled) :
            null;

    public PororocaWebSocketConnection ToWebSocketConnection() => new(
        Name: Name,
        HttpVersion: HttpVersion,
        Url: Url,
        CustomAuth: RequestAuthDataCtx.ToCustomAuth(),
        Headers: RequestHeadersTableVm.Items.Count == 0 ? null : RequestHeadersTableVm.ConvertItemsToDomain(),
        ClientMessages: Items.Count == 0 ? null : Items.Select(i => i.ToWebSocketClientMessage()).ToList(),
        Subprotocols: SubprotocolsTableVm.Items.Count == 0 ? null : SubprotocolsTableVm.ConvertItemsToDomain(),
        CompressionOptions: WrapCompressionOptionsFromInputs());

    #endregion

    #region CONNECT / DISCONNECT

    private void ClearInvalidConnectionWarnings()
    {
        this.invalidConnectionErrorCodeField = null;
        IsInvalidConnectionErrorVisible = false;
        InvalidConnectionError = null;

        HasUrlValidationProblem = false;
        HasHttpVersionValidationProblem = false;

        RequestAuthDataCtx.ClearRequestAuthValidationWarnings();
    }

    private void OnWebSocketConnectionChanged(PororocaWebSocketConnectorState state, Exception? ex)
    {
        ConnectionState = state;
        ConnectionExceptionContent = ex?.ToString();
        if (state == PororocaWebSocketConnectorState.Connected || ex is not null)
        {
            // TODO: do not use fixed integers here, find a better way
            SelectedConnectionTabIndex = 2;
        }
        IsDisableTlsVerificationVisible = IsInvalidTlsCertificateWsException(ex);
    }

    private static bool IsInvalidTlsCertificateWsException(Exception? ex) =>
        ex?.InnerException?.InnerException is AuthenticationException aex
     && aex.Message.Contains("remote certificate is invalid", StringComparison.InvariantCultureIgnoreCase);

    private void OnWebSocketMessageSending(bool isSendingAMessage) =>
        IsSendingAMessage = isSendingAMessage;

    public Task ConnectDisconnectCancelAsync()
    {
        switch (ConnectionState)
        {
            case PororocaWebSocketConnectorState.Disconnected:
                return ConnectAsync();
            case PororocaWebSocketConnectorState.Connecting:
                CancelConnect();
                return Task.CompletedTask;
            case PororocaWebSocketConnectorState.Connected:
                return DisconnectAsync();
            case PororocaWebSocketConnectorState.Disconnecting:
                CancelDisconnect();
                return Task.CompletedTask;
            default:
                return Task.CompletedTask;
        }
    }

    public async Task ConnectAsync()
    {
        var wsConn = ToWebSocketConnection();
        var effectiveVars = ((IPororocaVariableResolver)this.col).GetEffectiveVariables();
        bool disableTlsVerification = MainWindowVm.IsSslVerificationDisabled;

        if (!IsValidConnection(effectiveVars, this.col.CollectionScopedAuth, wsConn, out var resolvedUri, out string? translateUriErrorCode))
        {
            InvalidConnectionErrorCode = translateUriErrorCode;
        }
        else if (!TryTranslateConnection(effectiveVars, this.col.CollectionScopedAuth, this.col.CollectionScopedRequestHeaders, this.httpClientProvider, wsConn, disableTlsVerification,
                                         out var resolvedClients, out string? translateConnErrorCode))
        {
            InvalidConnectionErrorCode = translateConnErrorCode;
        }
        else
        {
            InvalidConnectionErrorCode = null;
            ExchangedMessages.Clear();
            this.cancelConnectionAttemptTokenSource = new();
            // This needs to be done in a different thread.
            // Awaiting the request.RequestAsync() here, or simply returning its Task,
            // causes the UI to freeze for a few seconds, especially when performing the first request to a server.
            // That is why we are invoking the code to run in a new thread, like below.
            await Task.Run(async () =>
            {
                await this.connector.ConnectAsync(resolvedClients.wsCli!, resolvedClients.httpCli!, resolvedUri!, this.cancelConnectionAttemptTokenSource.Token);
                Dispatcher.UIThread.Post(async () =>
                {
                    WasConnectionSuccessful = this.connector.ConnectionException is null;
                    ResponseStatusCodeElapsedTimeTitle = FormatResponseTitle(this.connector.ElapsedConnectionTimeSpan, resolvedClients.wsCli!.HttpStatusCode);
                    ConnectionResponseHeadersTableVm.Items.Clear();
                    var resHeaders = this.connector.ConnectionHttpHeaders?.SelectMany(
                        kv => kv.Value.Select(val => new KeyValueParamViewModel(ConnectionResponseHeadersTableVm.Items, true, kv.Key, val))).ToList()
                        ?? new List<KeyValueParamViewModel>();
                    foreach (var header in resHeaders)
                    {
                        ConnectionResponseHeadersTableVm.Items.Add(header);
                    }
                    if (WasConnectionSuccessful)
                    {
                        await CollectExchangedMessagesAsync(this.connector.ExchangedMessagesCollector!);
                    }
                });
            });
        }
    }

    public void CancelConnect() =>
        this.cancelConnectionAttemptTokenSource?.Cancel();

    public Task DisconnectAsync()
    {
        this.cancelDisconnectionAttemptTokenSource = new();
        // This needs to be done in a different thread.
        // Awaiting the request.RequestAsync() here, or simply returning its Task,
        // causes the UI to freeze for a few seconds, especially when performing the first request to a server.
        // That is why we are invoking the code to run in a new thread, like below.
        return Task.Run(() => this.connector.DisconnectAsync(this.cancelDisconnectionAttemptTokenSource.Token));
    }

    private void CancelDisconnect() =>
        this.cancelDisconnectionAttemptTokenSource?.Cancel();

    private void DisableTlsVerification()
    {
        MainWindowVm.IsSslVerificationDisabled = true;
        IsDisableTlsVerificationVisible = false;
    }

    private static string FormatResponseTitle(TimeSpan elapsedTime, HttpStatusCode statusCode) =>
        string.Format("{0} ({1})",
            FormatHttpStatusCodeText(statusCode),
            FormatTimeText(elapsedTime));

    #endregion

    #region EXCHANGED MESSAGES

    private async Task SendMessageAsync()
    {
        if (ConnectionState != PororocaWebSocketConnectorState.Connected)
        {
            InvalidClientMessageErrorCode = TranslateRequestErrors.WebSocketNotConnected;
        }
        else
        {
            var msg = Items[MessageToSendSelectedIndex].ToWebSocketClientMessage();
            var effectiveVars = ((IPororocaVariableResolver)this.col).GetEffectiveVariables();
            if (!IsValidClientMessage(effectiveVars, msg, out string? validationErrorCode))
            {
                InvalidClientMessageErrorCode = validationErrorCode;
            }
            else if (!TryTranslateClientMessage(effectiveVars, msg, out var resolvedMsgToSend, out string? translationErrorCode))
            {
                InvalidClientMessageErrorCode = translationErrorCode;
            }
            else
            {
                InvalidClientMessageErrorCode = null;
                await this.connector.SendMessageAsync(resolvedMsgToSend!);
            }
        }
    }

    private async Task CollectExchangedMessagesAsync(ChannelReader<PororocaWebSocketMessage> channelReader)
    {
        await foreach (var msg in channelReader.ReadAllAsync())
        {
            ExchangedMessages.Insert(0, new(msg));
        }
    }

    #endregion

    #region MESSAGE DETAIL

    private void OnSelectedExchangedMessageChanged(WebSocketExchangedMessageViewModel? vm)
    {
        SelectedExchangedMessageType = vm?.TypeDescription;
        SelectedExchangedMessageContent = vm?.TextContent;
        IsSaveSelectedExchangedMessageToFileVisible = vm?.CanBeSavedToFile ?? false;
    }

    public Task SaveSelectedExchangedMessageToFileAsync()
    {
        if (SelectedExchangedMessage is not null && SelectedExchangedMessage.CanBeSavedToFile)
        {
            return SaveExchangedMessageToFileAsync(SelectedExchangedMessage);
        }
        else
        {
            return Task.CompletedTask;
        }
    }

    private async Task SaveExchangedMessageToFileAsync(WebSocketExchangedMessageViewModel exchangedMessage)
    {
        static string GenerateDefaultInitialFileName(string wsName, int index, WebSocketExchangedMessageViewModel vm)
        {
            string fileExtensionWithoutDot = vm.IsJsonTextContent ? "json" : "txt";
            string from = vm.IsFromClient ? "fromClient" : "fromServer";

            return $"ws-{wsName}-msg{index}-{from}-{vm.ShortInstantDescription}.{fileExtensionWithoutDot}";
        }
        // WARNING: WebSocket messages that came from the client
        // and are file-based cannot be saved.
        int index = ExchangedMessages.Contains(exchangedMessage) ?
                    ExchangedMessages.Count - ExchangedMessages.IndexOf(exchangedMessage) :
                    0;
        string initialFileName = GenerateDefaultInitialFileName(Name, index, exchangedMessage);

        string? saveFileOutputPath = await FileExporterImporter.SelectPathForFileToBeSavedAsync(initialFileName);
        if (saveFileOutputPath != null)
        {
            const int fileStreamBufferSize = 4096;
            using FileStream fs = new(saveFileOutputPath, FileMode.Create, FileAccess.Write, FileShare.None, fileStreamBufferSize, useAsync: true);
            await fs.WriteAsync((Memory<byte>)exchangedMessage.Bytes!);
        }
    }

    #endregion
}