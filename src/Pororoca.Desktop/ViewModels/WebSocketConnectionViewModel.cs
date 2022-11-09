using System.Collections.ObjectModel;
using System.Globalization;
using System.Reactive;
using Avalonia.Controls;
using Pororoca.Desktop.Localization;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.Requester;
using Pororoca.Domain.Features.TranslateRequest;
using Pororoca.Domain.Features.VariableResolution;
using Pororoca.Infrastructure.Features.Requester;
using ReactiveUI;
using static Pororoca.Domain.Features.Common.AvailablePororocaRequestSelectionOptions;
using static Pororoca.Domain.Features.TranslateRequest.WebSockets.Connection.PororocaWebSocketConnectionValidator;
using static Pororoca.Domain.Features.TranslateRequest.WebSockets.Connection.PororocaWebSocketConnectionTranslator;
using static Pororoca.Domain.Features.TranslateRequest.WebSockets.ClientMessage.PororocaWebSocketClientMessageTranslator;
using static Pororoca.Domain.Features.TranslateRequest.WebSockets.ClientMessage.PororocaWebSocketClientMessageValidator;
using System.Buffers;
using System.Collections.Specialized;
using System.Security.Authentication;
using System.Text.Json;

namespace Pororoca.Desktop.ViewModels;

public sealed class WebSocketConnectionViewModel : CollectionOrganizationItemParentViewModel<WebSocketClientMessageViewModel>
{
    #region COLLECTION ORGANIZATION

    public override Action OnAfterItemDeleted => Parent.OnAfterItemDeleted;
    public override Action<CollectionOrganizationItemViewModel> OnRenameSubItemSelected => Parent.OnRenameSubItemSelected;
    public ReactiveCommand<Unit, Unit> AddNewWebSocketClientMessageCmd { get; }
    public ReactiveCommand<Unit, Unit> PasteToWebSocketConnectionCmd { get; }
    public ReactiveCommand<Unit, Unit> CopyCmd { get; }
    public ReactiveCommand<Unit, Unit> RenameCmd { get; }
    public ReactiveCommand<Unit, Unit> MoveUpCmd { get; }
    public ReactiveCommand<Unit, Unit> MoveDownCmd { get; }
    public ReactiveCommand<Unit, Unit> DeleteCmd { get; }

    #endregion

    #region CONNECTION

    private readonly IPororocaVariableResolver varResolver;
    private readonly IPororocaClientCertificatesProvider clientCertsProvider;
    private readonly Guid wsId;
    private readonly PororocaWebSocketConnector connector;

    private bool isConnectedField;
    public bool IsConnected
    {
        get => this.isConnectedField;
        set
        {
            NameEditableTextBlockViewDataCtx.IsConnectedWebSocket = value;
            NameEditableTextBlockViewDataCtx.IsDisconnectedWebSocket = !value;
            this.RaiseAndSetIfChanged(ref this.isConnectedField, value);
        }
    }

    private bool isConnectingField;
    public bool IsConnecting
    {
        get => this.isConnectingField;
        set
        {
            this.RaiseAndSetIfChanged(ref this.isConnectingField, value);
            IsConnectingOrDisconnecting = this.isConnectingField || this.isDisconnectingField;
        }
    }

    private bool isDisconnectingField;
    public bool IsDisconnecting
    {
        get => this.isDisconnectingField;
        set
        {
            this.RaiseAndSetIfChanged(ref this.isDisconnectingField, value);
            IsConnectingOrDisconnecting = this.isConnectingField || this.isDisconnectingField;
        }
    }

    private bool isConnectingOrDisconnectingField;
    public bool IsConnectingOrDisconnecting
    {
        get => this.isConnectingOrDisconnectingField;
        set => this.RaiseAndSetIfChanged(ref this.isConnectingOrDisconnectingField, value);
    }

    private bool isDisableTlsVerificationVisibleField;
    public bool IsDisableTlsVerificationVisible
    {
        get => this.isDisableTlsVerificationVisibleField;
        set => this.RaiseAndSetIfChanged(ref this.isDisableTlsVerificationVisibleField, value);
    }

    public ReactiveCommand<Unit, Unit> ConnectCmd { get; }

    private CancellationTokenSource? cancelConnectionAttemptTokenSource;

    public ReactiveCommand<Unit, Unit> CancelConnectCmd { get; }

    public ReactiveCommand<Unit, Unit> DisconnectCmd { get; }

    private CancellationTokenSource? cancelDisconnectionAttemptTokenSource;

    public ReactiveCommand<Unit, Unit> CancelDisconnectCmd { get; }

    public ReactiveCommand<Unit, Unit> DisableTlsVerificationCmd { get; }

    private int selectedConnectionTabIndexField;
    public int SelectedConnectionTabIndex // To preserve the state of the last shown request tab
    {
        get => this.selectedConnectionTabIndexField;
        set => this.RaiseAndSetIfChanged(ref this.selectedConnectionTabIndexField, value);
    }

    private string urlField;
    public string Url
    {
        get => this.urlField;
        set => this.RaiseAndSetIfChanged(ref this.urlField, value);
    }

    private string resolvedUrlToolTipField;
    public string ResolvedUrlToolTip
    {
        get => this.resolvedUrlToolTipField;
        set => this.RaiseAndSetIfChanged(ref this.resolvedUrlToolTipField, value);
    }

    public override ObservableCollection<WebSocketClientMessageViewModel> Items { get; } = new();

    #endregion

    #region CONNECTION REQUEST HTTP VERSION

    public ObservableCollection<string> HttpVersionSelectionOptions { get; }
    private int httpVersionSelectedIndexField;
    public int HttpVersionSelectedIndex
    {
        get => this.httpVersionSelectedIndexField;
        set => this.RaiseAndSetIfChanged(ref this.httpVersionSelectedIndexField, value);
    }
    private decimal HttpVersion =>
        AvailableHttpVersionsForWebSockets[this.httpVersionSelectedIndexField];

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
                TranslateRequestErrors.ClientCertificateFileNotFound => Localizer.Instance["RequestValidation/ClientCertificateFileNotFound"],
                TranslateRequestErrors.ClientCertificatePkcs12PasswordCannotBeBlank => Localizer.Instance["RequestValidation/ClientCertificatePkcs12PasswordCannotBeBlank"],
                TranslateRequestErrors.ClientCertificatePrivateKeyFileNotFound => Localizer.Instance["RequestValidation/ClientCertificatePrivateKeyFileNotFound"],
                TranslateRequestErrors.InvalidUrl => Localizer.Instance["RequestValidation/InvalidUrl"],
                TranslateRequestErrors.WebSocketHttpVersionUnavailable => Localizer.Instance["RequestValidation/WebSocketHttpVersionUnavailable"],
                TranslateRequestErrors.WebSocketCompressionMaxWindowBitsOutOfRange => Localizer.Instance["RequestValidation/WebSocketCompressionMaxWindowBitsOutOfRange"],
                TranslateRequestErrors.WebSocketUnknownConnectionTranslationError => Localizer.Instance["RequestValidation/WebSocketUnknownConnectionTranslationError"],
                _ => Localizer.Instance["RequestValidation/WebSocketUnknownConnectionTranslationError"]
            };
        }
    }

    private bool isInvalidConnectionErrorVisibleField;
    public bool IsInvalidConnectionErrorVisible
    {
        get => this.isInvalidConnectionErrorVisibleField;
        set => this.RaiseAndSetIfChanged(ref this.isInvalidConnectionErrorVisibleField, value);
    }
    private string? invalidConnectionErrorField;
    public string? InvalidConnectionError
    {
        get => this.invalidConnectionErrorField;
        set => this.RaiseAndSetIfChanged(ref this.invalidConnectionErrorField, value);
    }

    #endregion

    #region CONNECTION OPTIONS

    private int connectionOptionSelectedIndexField;
    public int ConnectionOptionSelectedIndex
    {
        get => this.connectionOptionSelectedIndexField;
        set => this.RaiseAndSetIfChanged(ref this.connectionOptionSelectedIndexField, value);
    }

    private bool isConnectionOptionHeadersSelectedField;
    public bool IsConnectionOptionHeadersSelected
    {
        get => this.isConnectionOptionHeadersSelectedField;
        set => this.RaiseAndSetIfChanged(ref this.isConnectionOptionHeadersSelectedField, value);
    }

    private bool isConnectionOptionSubprotocolsSelectedField;
    public bool IsConnectionOptionSubprotocolsSelected
    {
        get => this.isConnectionOptionSubprotocolsSelectedField;
        set => this.RaiseAndSetIfChanged(ref this.isConnectionOptionSubprotocolsSelectedField, value);
    }

    private bool isConnectionOptionCompressionSelectedField;
    public bool IsConnectionOptionCompressionSelected
    {
        get => this.isConnectionOptionCompressionSelectedField;
        set => this.RaiseAndSetIfChanged(ref this.isConnectionOptionCompressionSelectedField, value);
    }

    #region CONNECTION OPTION HEADERS

    public ObservableCollection<KeyValueParamViewModel> ConnectionRequestHeaders { get; }
    public KeyValueParamViewModel? SelectedConnectionRequestHeader { get; set; }
    public ReactiveCommand<Unit, Unit> AddNewConnectionRequestHeaderCmd { get; }
    public ReactiveCommand<Unit, Unit> RemoveSelectedConnectionRequestHeaderCmd { get; }

    #endregion

    #region CONNECTION OPTION SUBPROTOCOLS

    public ObservableCollection<KeyValueParamViewModel> Subprotocols { get; }
    public KeyValueParamViewModel? SelectedSubprotocol { get; set; }
    public ReactiveCommand<Unit, Unit> AddNewSubprotocolCmd { get; }
    public ReactiveCommand<Unit, Unit> RemoveSelectedSubprotocolCmd { get; }

    #endregion

    #region CONNECTION OPTION COMPRESSION

    private bool enableCompressionField;
    public bool EnableCompression
    {
        get => this.enableCompressionField;
        set => this.RaiseAndSetIfChanged(ref this.enableCompressionField, value);
    }

    private bool compressionClientContextTakeoverEnabledField;
    public bool CompressionClientContextTakeoverEnabled
    {
        get => this.compressionClientContextTakeoverEnabledField;
        set => this.RaiseAndSetIfChanged(ref this.compressionClientContextTakeoverEnabledField, value);
    }

    private int compressionClientMaxWindowBitsField;
    public int CompressionClientMaxWindowBits
    {
        get => this.compressionClientMaxWindowBitsField;
        set => this.RaiseAndSetIfChanged(ref this.compressionClientMaxWindowBitsField, value);
    }

    private bool compressionServerContextTakeoverEnabledField;
    public bool CompressionServerContextTakeoverEnabled
    {
        get => this.compressionServerContextTakeoverEnabledField;
        set => this.RaiseAndSetIfChanged(ref this.compressionServerContextTakeoverEnabledField, value);
    }

    private int compressionServerMaxWindowBitsField;
    public int CompressionServerMaxWindowBits
    {
        get => this.compressionServerMaxWindowBitsField;
        set => this.RaiseAndSetIfChanged(ref this.compressionServerMaxWindowBitsField, value);
    }

    #endregion

    #endregion

    #region CONNECTION REQUEST AUTH

    private RequestAuthViewModel requestAuthDataCtxField;
    public RequestAuthViewModel RequestAuthDataCtx
    {
        get => this.requestAuthDataCtxField;
        set => this.RaiseAndSetIfChanged(ref this.requestAuthDataCtxField, value);
    }

    #endregion

    #region CONNECTION EXCEPTION

    private string? connectionExceptionContentField;
    public string? ConnectionExceptionContent
    {
        get => this.connectionExceptionContentField;
        set => this.RaiseAndSetIfChanged(ref this.connectionExceptionContentField, value);
    }

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
                TranslateRequestErrors.WebSocketNotConnected  => Localizer.Instance["RequestValidation/WebSocketNotConnected"],
                TranslateRequestErrors.WebSocketClientMessageContentFileNotFound  => Localizer.Instance["RequestValidation/WebSocketClientMessageContentFileNotFound"],
                TranslateRequestErrors.WebSocketUnknownClientMessageTranslationError  => Localizer.Instance["RequestValidation/WebSocketUnknownClientMessageTranslationError"],
                _ => Localizer.Instance["RequestValidation/WebSocketUnknownClientMessageTranslationError"]
            };
        }
    }

    private bool isInvalidClientMessageErrorVisibleField;
    public bool IsInvalidClientMessageErrorVisible
    {
        get => this.isInvalidClientMessageErrorVisibleField;
        set => this.RaiseAndSetIfChanged(ref this.isInvalidClientMessageErrorVisibleField, value);
    }
    private string? invalidClientMessageErrorField;
    public string? InvalidClientMessageError
    {
        get => this.invalidClientMessageErrorField;
        set => this.RaiseAndSetIfChanged(ref this.invalidClientMessageErrorField, value);
    }

    private bool isSendingAMessageField;
    public bool IsSendingAMessage
    {
        get => this.isSendingAMessageField;
        set => this.RaiseAndSetIfChanged(ref this.isSendingAMessageField, value);
    }

    public ObservableCollection<WebSocketExchangedMessageViewModel> ExchangedMessages { get; }
    
    private int messageToSendSelectedIndexField;
    public int MessageToSendSelectedIndex
    {
        get => this.messageToSendSelectedIndexField;
        set => this.RaiseAndSetIfChanged(ref this.messageToSendSelectedIndexField, value);
    }

    public ReactiveCommand<Unit, Unit> SendMessageCmd { get; }

    #endregion

    #region MESSAGE DETAIL

    private WebSocketExchangedMessageViewModel? selectedExchangedMessage;

    private string? selectedExchangedMessageTypeField;
    public string? SelectedExchangedMessageType
    {
        get => this.selectedExchangedMessageTypeField;
        set => this.RaiseAndSetIfChanged(ref this.selectedExchangedMessageTypeField, value);
    }

    public string? SelectedExchangedMessageContent { get; set; }
    public bool IsSelectedExchangedMessageContentJson { get; set; }

    private bool isSaveSelectedExchangedMessageToFileVisibleField;
    public bool IsSaveSelectedExchangedMessageToFileVisible
    {
        get => this.isSaveSelectedExchangedMessageToFileVisibleField;
        set => this.RaiseAndSetIfChanged(ref this.isSaveSelectedExchangedMessageToFileVisibleField, value);
    }

    public ReactiveCommand<Unit, Unit> SaveSelectedExchangedMessageToFileCmd { get; }

    #endregion

    public WebSocketConnectionViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                                        IPororocaVariableResolver variableResolver,
                                        PororocaWebSocketConnection ws) : base(parentVm, ws.Name)
    {
        #region COLLECTION ORGANIZATION
        Localizer.Instance.SubscribeToLanguageChange(OnLanguageChanged);

        NameEditableTextBlockViewDataCtx.IsDisconnectedWebSocket = true;
        AddNewWebSocketClientMessageCmd = ReactiveCommand.Create(AddNewWebSocketClientMessage);
        PasteToWebSocketConnectionCmd = ReactiveCommand.Create(PasteToThis);
        CopyCmd = ReactiveCommand.Create(Copy);
        RenameCmd = ReactiveCommand.Create(RenameThis);
        MoveUpCmd = ReactiveCommand.Create(MoveThisUp);
        MoveDownCmd = ReactiveCommand.Create(MoveThisDown);
        DeleteCmd = ReactiveCommand.Create(Delete);
        #endregion

        #region CONNECTION

        this.varResolver = variableResolver;
        this.clientCertsProvider = PororocaClientCertificatesProvider.Singleton;
        this.wsId = ws.Id;
        this.connector = new(OnWebSocketConnectionChanged, OnWebSocketMessageSending);

        if (ws.ClientMessages is not null)
        {
            foreach (var wsReqMsg in ws.ClientMessages)
                Items.Add(new(this, wsReqMsg));

            RefreshSubItemsAvailableMovements();
        }

        ConnectCmd = ReactiveCommand.CreateFromTask(ConnectAsync);
        CancelConnectCmd = ReactiveCommand.Create(CancelConnect);
        DisconnectCmd = ReactiveCommand.CreateFromTask(DisconnectAsync);
        CancelDisconnectCmd = ReactiveCommand.Create(CancelDisconnect);
        DisableTlsVerificationCmd = ReactiveCommand.Create(DisableTlsVerification);

        #endregion

        #region CONNECTION REQUEST HTTP VERSION AND URL
        this.resolvedUrlToolTipField = this.urlField = ws.Url;

        HttpVersionSelectionOptions = new(AvailableHttpVersionsForWebSockets.Select(FormatHttpVersionString));
        int httpVersionSelectionIndex = HttpVersionSelectionOptions.IndexOf(FormatHttpVersionString(ws.HttpVersion));
        HttpVersionSelectedIndex = httpVersionSelectionIndex >= 0 ? httpVersionSelectionIndex : 0;
        #endregion

        #region CONNECTION REQUEST AUTH
        this.requestAuthDataCtxField = new(ws.CustomAuth);
        #endregion

        #region CONNECTION OPTIONS

        ConnectionOptionSelectedIndex = 0;
        IsConnectionOptionHeadersSelected = true;
        IsConnectionOptionSubprotocolsSelected = false;
        IsConnectionOptionCompressionSelected = false;

        #region CONNECTION OPTION HEADERS

        ConnectionRequestHeaders = new(ws.Headers?.Select(h => new KeyValueParamViewModel(h)) ?? Array.Empty<KeyValueParamViewModel>());
        AddNewConnectionRequestHeaderCmd = ReactiveCommand.Create(AddNewConnectionRequestHeader);
        RemoveSelectedConnectionRequestHeaderCmd = ReactiveCommand.Create(RemoveSelectedConnectionRequestHeader);

        #endregion

        #region CONNECTION OPTION SUBPROTOCOLS

        Subprotocols = new(ws.Subprotocols?.Select(s => new KeyValueParamViewModel(s)) ?? Array.Empty<KeyValueParamViewModel>());
        AddNewSubprotocolCmd = ReactiveCommand.Create(AddNewSubprotocol);
        RemoveSelectedSubprotocolCmd = ReactiveCommand.Create(RemoveSelectedSubprotocol);

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
        ExchangedMessages = new();
        this.connector.ExchangedMessages.CollectionChanged += OnConnectorExchangedMessagesUpdated;
        SendMessageCmd = ReactiveCommand.CreateFromTask(SendMessageAsync);
        #endregion

        #region MESSAGE DETAIL
        SaveSelectedExchangedMessageToFileCmd = ReactiveCommand.CreateFromTask(SaveSelectedExchangedMessageToFileAsync);
        #endregion
    }

    #region COLLECTION ORGANIZATION

    public override void RefreshSubItemsAvailableMovements()
    {
        for (int x = 0; x < Items.Count; x++)
        {
            var colItemVm = Items[x];
            bool canMoveUp = x > 0;
            bool canMoveDown = x < Items.Count - 1;
            colItemVm.CanMoveUp = canMoveUp;
            colItemVm.CanMoveDown = canMoveDown;
        }
    }

    protected override void CopyThis() =>
        CollectionsGroupDataCtx.PushToCopy(ToWebSocketConnection());

    public override void PasteToThis()
    {
        var itemsToPaste = CollectionsGroupDataCtx.FetchCopiesOfWebSocketClientMessages();
        foreach (var itemToPaste in itemsToPaste)
        {
            AddWebSocketClientMessage(itemToPaste);
        }
    }

    private void AddNewWebSocketClientMessage()
    {
        PororocaWebSocketClientMessage wsReqMsg = new(PororocaWebSocketMessageType.Text,
                                                       Localizer.Instance["WebSocketClientMessage/NewMessage"],
                                                       PororocaWebSocketClientMessageContentMode.Raw,
                                                       string.Empty,
                                                       PororocaWebSocketMessageRawContentSyntax.Json,
                                                       null,
                                                       false);
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
    }

    #endregion

    #region REQUEST HTTP METHOD, HTTP VERSION AND URL

    public void UpdateResolvedUrlToolTip() =>
        ResolvedUrlToolTip = this.varResolver.ReplaceTemplates(Url);

    private static string FormatHttpVersionString(decimal httpVersion) =>
        httpVersion switch
        {
            1.0m => "HTTP/1.0",
            1.1m => "HTTP/1.1",
            2.0m => "HTTP/2",
            3.0m => "HTTP/3",
            _ => string.Format(CultureInfo.InvariantCulture, "HTTP/{0:0.0}", httpVersion)
        };

    #endregion

    #region CONNECTION REQUEST HEADERS

    private void AddNewConnectionRequestHeader() =>
        ConnectionRequestHeaders.Add(new(true, string.Empty, string.Empty));

    private void RemoveSelectedConnectionRequestHeader()
    {
        if (SelectedConnectionRequestHeader != null)
        {
            ConnectionRequestHeaders.Remove(SelectedConnectionRequestHeader);
            SelectedConnectionRequestHeader = null;
        }
        else if (ConnectionRequestHeaders.Count == 1)
        {
            ConnectionRequestHeaders.Clear();
        }
    }

    #endregion

    #region CONNECTION OPTION SUBPROTOCOLS

    private void AddNewSubprotocol() =>
        Subprotocols.Add(new(true, string.Empty, string.Empty));

    private void RemoveSelectedSubprotocol()
    {
        if (SelectedSubprotocol != null)
        {
            Subprotocols.Remove(SelectedSubprotocol);
            SelectedSubprotocol = null;
        }
        else if (Subprotocols.Count == 1)
        {
            Subprotocols.Clear();
        }
    }

    #endregion

    #region CONVERT VIEW INPUTS TO REQUEST ENTITY

    private PororocaWebSocketCompressionOptions? WrapCompressionOptionsFromInputs() =>
        EnableCompression ?
            new(CompressionClientMaxWindowBits,
                CompressionClientContextTakeoverEnabled,
                CompressionServerMaxWindowBits,
                CompressionServerContextTakeoverEnabled) :
            null;

    public PororocaWebSocketConnection ToWebSocketConnection()
    {
        PororocaWebSocketConnection newWs = new(this.wsId, Name);
        UpdateConnectionWithInputs(newWs);
        return newWs;
    }

    private void UpdateConnectionWithInputs(PororocaWebSocketConnection ws)
    {
        ws.UpdateName(Name);
        ws.HttpVersion = HttpVersion;
        ws.Url = Url;
        ws.CustomAuth = this.requestAuthDataCtxField.ToCustomAuth();
        ws.Headers = ConnectionRequestHeaders.Count == 0 ? null : ConnectionRequestHeaders.Select(h => h.ToKeyValueParam()).ToList();
        ws.ClientMessages = Items.Count == 0 ? null : Items.Select(i => i.ToWebSocketClientMessage()).ToList();
        ws.Subprotocols = Subprotocols.Count == 0 ? null : Subprotocols.Select(s => s.ToKeyValueParam()).ToList();
        ws.CompressionOptions = WrapCompressionOptionsFromInputs();
    }

    #endregion

    #region CONNECT / DISCONNECT

    private void OnWebSocketConnectionChanged(PororocaWebSocketConnectorState state, Exception? ex)
    {
        IsConnecting = state == PororocaWebSocketConnectorState.Connecting;
        IsDisconnecting = state == PororocaWebSocketConnectorState.Disconnecting;
        IsConnected = state == PororocaWebSocketConnectorState.Connected;
        ConnectionExceptionContent = ex?.ToString();        
        if (ex is not null)
        {
            // switching to Exception tab
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

    private async Task ConnectAsync()
    {        
        var wsConn = ToWebSocketConnection();
        bool disableTlsVerification = ((MainWindowViewModel)MainWindow.Instance!.DataContext!).IsSslVerificationDisabled;

        if (!IsValidConnection(this.varResolver, wsConn, out var resolvedUri, out string? translateUriErrorCode))
        {
            InvalidConnectionErrorCode = translateUriErrorCode;
        }
        else if (!TryTranslateConnection(this.varResolver, this.clientCertsProvider, wsConn, disableTlsVerification,
                                         out var resolvedClient, out string? translateConnErrorCode))
        {
            InvalidConnectionErrorCode = translateConnErrorCode;
        }
        else
        {
            InvalidConnectionErrorCode = null;
            this.cancelConnectionAttemptTokenSource = new();
            await this.connector.ConnectAsync(resolvedClient!, resolvedUri!, this.cancelConnectionAttemptTokenSource.Token);
        }
    }

    private void CancelConnect() =>
        this.cancelConnectionAttemptTokenSource?.Cancel();

    private Task DisconnectAsync()
    {
        this.cancelDisconnectionAttemptTokenSource = new();
        return this.connector.DisconnectAsync(this.cancelDisconnectionAttemptTokenSource.Token);        
    }

    private void CancelDisconnect() =>
        this.cancelDisconnectionAttemptTokenSource?.Cancel();

    private void DisableTlsVerification()
    {
        ((MainWindowViewModel)MainWindow.Instance!.DataContext!).IsSslVerificationDisabled = true;
        IsDisableTlsVerificationVisible = false;
    }

    #endregion

    #region EXCHANGED MESSAGES

    private async Task SendMessageAsync()
    {
        if (!IsConnected)
        {
            InvalidClientMessageErrorCode = TranslateRequestErrors.WebSocketNotConnected;
        }
        else
        {
            var msg = Items[this.messageToSendSelectedIndexField].ToWebSocketClientMessage();

            if (!IsValidClientMessage(this.varResolver, msg, out string? validationErrorCode))
            {
                InvalidClientMessageErrorCode = validationErrorCode;
            }
            else if (!TryTranslateClientMessage(this.varResolver, msg, out var resolvedMsgToSend, out string? translationErrorCode))
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

    private void OnConnectorExchangedMessagesUpdated(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            ExchangedMessages.Clear();
        }
        else if (e.Action == NotifyCollectionChangedAction.Add)
        {
            foreach (object newItem in e.NewItems!)
            {
                var msg = (PororocaWebSocketMessage) newItem;
                ExchangedMessages.Insert(0, new(msg));
            }
        }
    }

    #endregion


    #region MESSAGE DETAIL

    public void UpdateSelectedExchangedMessage(WebSocketExchangedMessageViewModel vm)
    {
        this.selectedExchangedMessage = vm;
        SelectedExchangedMessageType = vm.TypeDescription;
        SelectedExchangedMessageContent = vm.TextContent;
        IsSelectedExchangedMessageContentJson = IsJsonString(SelectedExchangedMessageContent);
        IsSaveSelectedExchangedMessageToFileVisible = vm.CanBeSavedToFile;
    }

    private async Task SaveSelectedExchangedMessageToFileAsync()
    {
        static string GenerateDefaultInitialFileName(WebSocketExchangedMessageViewModel vm)
        {
            string fileExtensionWithoutDot = vm.Type == PororocaWebSocketMessageType.Text ?
                                             "txt" :
                                             string.Empty;

            return $"websocket-msg-{vm.ShortInstantDescription}.{fileExtensionWithoutDot}";
        }

        if (this.selectedExchangedMessage is not null && this.selectedExchangedMessage.CanBeSavedToFile)
        {
            SaveFileDialog saveFileDialog = new()
            {
                InitialFileName = GenerateDefaultInitialFileName(this.selectedExchangedMessage)
            };

            string? saveFileOutputPath = await saveFileDialog.ShowAsync(MainWindow.Instance!);
            if (saveFileOutputPath != null)
            {
                const int fileStreamBufferSize = 4096;
                using FileStream fs = new(saveFileOutputPath, FileMode.Create, FileAccess.Write, FileShare.None, fileStreamBufferSize, useAsync: true);
                await fs.WriteAsync((Memory<byte>) this.selectedExchangedMessage.Bytes!);
            }
        }
    }

    private static bool IsJsonString(string? str)
    {
        if (!string.IsNullOrWhiteSpace(str))
        {
            try
            {
                JsonSerializer.Deserialize<dynamic>(str);
                return true;
            }
            catch
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    #endregion
}