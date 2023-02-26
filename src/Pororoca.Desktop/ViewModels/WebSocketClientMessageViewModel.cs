using System.Reactive;
using Avalonia.Controls;
using AvaloniaEdit.Document;
using Pororoca.Desktop.ExportImport;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using ReactiveUI;

namespace Pororoca.Desktop.ViewModels;

public sealed class WebSocketClientMessageViewModel : CollectionOrganizationItemViewModel
{
    #region COLLECTION ORGANIZATION

    public ReactiveCommand<Unit, Unit> CopyCmd { get; }
    public ReactiveCommand<Unit, Unit> RenameCmd { get; }
    public ReactiveCommand<Unit, Unit> MoveUpCmd { get; }
    public ReactiveCommand<Unit, Unit> MoveDownCmd { get; }
    public ReactiveCommand<Unit, Unit> DeleteCmd { get; }

    #endregion

    #region WEBSOCKET REQUEST MESSAGE

    private readonly Guid wsCliMsgId;

    private bool disableCompressionForThisMessageField;
    public bool DisableCompressionForThisMessage
    {
        get => this.disableCompressionForThisMessageField;
        set => this.RaiseAndSetIfChanged(ref this.disableCompressionForThisMessageField, value);
    }

    #region MESSAGE TYPE

    private int messageTypeSelectedIndexField;
    public int MessageTypeSelectedIndex
    {
        get => this.messageTypeSelectedIndexField;
        set => this.RaiseAndSetIfChanged(ref this.messageTypeSelectedIndexField, value);
    }

    private bool isMessageTypeTextSelectedField;
    public bool IsMessageTypeTextSelected
    {
        get => this.isMessageTypeTextSelectedField;
        set => this.RaiseAndSetIfChanged(ref this.isMessageTypeTextSelectedField, value);
    }

    private bool isMessageTypeBinarySelectedField;
    public bool IsMessageTypeBinarySelected
    {
        get => this.isMessageTypeBinarySelectedField;
        set => this.RaiseAndSetIfChanged(ref this.isMessageTypeBinarySelectedField, value);
    }

    private bool isMessageTypeCloseSelectedField;
    public bool IsMessageTypeCloseSelected
    {
        get => this.isMessageTypeCloseSelectedField;
        set => this.RaiseAndSetIfChanged(ref this.isMessageTypeCloseSelectedField, value);
    }

    private PororocaWebSocketMessageType MessageType
    {
        get
        {
            if (IsMessageTypeTextSelected)
                return PororocaWebSocketMessageType.Text;
            if (IsMessageTypeBinarySelected)
                return PororocaWebSocketMessageType.Binary;
            if (IsMessageTypeCloseSelected)
                return PororocaWebSocketMessageType.Close;
            else
                return PororocaWebSocketMessageType.Text;
        }
    }

    #endregion

    #region CONTENT

    private int contentModeSelectedIndexField;
    public int ContentModeSelectedIndex
    {
        get => this.contentModeSelectedIndexField;
        set => this.RaiseAndSetIfChanged(ref this.contentModeSelectedIndexField, value);
    }

    private bool isContentModeRawSelectedField;
    public bool IsContentModeRawSelected
    {
        get => this.isContentModeRawSelectedField;
        set => this.RaiseAndSetIfChanged(ref this.isContentModeRawSelectedField, value);
    }

    private bool isContentModeFileSelectedField;
    public bool IsContentModeFileSelected
    {
        get => this.isContentModeFileSelectedField;
        set => this.RaiseAndSetIfChanged(ref this.isContentModeFileSelectedField, value);
    }

    private PororocaWebSocketClientMessageContentMode ContentMode
    {
        get
        {
            if (IsContentModeRawSelected)
                return PororocaWebSocketClientMessageContentMode.Raw;
            if (IsContentModeFileSelected)
                return PororocaWebSocketClientMessageContentMode.File;
            else
                return PororocaWebSocketClientMessageContentMode.Raw;
        }
    }

    private TextDocument? rawContentTextDocumentField;
    public TextDocument? RawContentTextDocument
    {
        get => this.rawContentTextDocumentField;
        set => this.RaiseAndSetIfChanged(ref this.rawContentTextDocumentField, value);
    }

    public string? RawContent
    {
        get => RawContentTextDocument?.Text;
        set => RawContentTextDocument = new(value ?? string.Empty);
    }

    private int rawContentSyntaxSelectedIndexField;
    public int RawContentSyntaxSelectedIndex
    {
        get => this.rawContentSyntaxSelectedIndexField;
        set => this.RaiseAndSetIfChanged(ref this.rawContentSyntaxSelectedIndexField, value);
    }

    private bool isRawContentJsonSyntaxSelectedField;
    public bool IsRawContentJsonSyntaxSelected
    {
        get => this.isRawContentJsonSyntaxSelectedField;
        set => this.RaiseAndSetIfChanged(ref this.isRawContentJsonSyntaxSelectedField, value);
    }

    private bool isRawContentOtherSyntaxSelectedField;
    public bool IsRawContentOtherSyntaxSelected
    {
        get => this.isRawContentOtherSyntaxSelectedField;
        set => this.RaiseAndSetIfChanged(ref this.isRawContentOtherSyntaxSelectedField, value);
    }

    private PororocaWebSocketMessageRawContentSyntax? RawContentSyntax
    {
        get
        {
            if (IsRawContentJsonSyntaxSelected)
                return PororocaWebSocketMessageRawContentSyntax.Json;
            if (IsRawContentOtherSyntaxSelected)
                return PororocaWebSocketMessageRawContentSyntax.Other;
            else
                return null;
        }
    }

    private string? contentFileSrcPathField;
    public string? ContentFileSrcPath
    {
        get => this.contentFileSrcPathField;
        set => this.RaiseAndSetIfChanged(ref this.contentFileSrcPathField, value);
    }
    
    public ReactiveCommand<Unit, Unit> SearchContentFileCmd { get; }

    #endregion

    #endregion

    public WebSocketClientMessageViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                                           PororocaWebSocketClientMessage msg) : base(parentVm, msg.Name)
    {
        #region COLLECTION ORGANIZATION

        CopyCmd = ReactiveCommand.Create(Copy);
        RenameCmd = ReactiveCommand.Create(RenameThis);
        MoveUpCmd = ReactiveCommand.Create(MoveThisUp);
        MoveDownCmd = ReactiveCommand.Create(MoveThisDown);
        DeleteCmd = ReactiveCommand.Create(Delete);

        #endregion

        #region WEBSOCKET REQUEST MESSAGE

        this.wsCliMsgId = msg.Id;
        DisableCompressionForThisMessage = msg.DisableCompressionForThis;
        switch (msg.MessageType)
        {
            // TODO: Improve this, do not use fixed values to resolve index
            default:
            case PororocaWebSocketMessageType.Text:
                MessageTypeSelectedIndex = 0;
                IsMessageTypeTextSelected = true;
                break;
            case PororocaWebSocketMessageType.Binary:
                MessageTypeSelectedIndex = 1;
                IsMessageTypeBinarySelected = true;
                break;
            case PororocaWebSocketMessageType.Close:
                MessageTypeSelectedIndex = 2;
                IsMessageTypeCloseSelected = true;
                break;
        }
        switch (msg.ContentMode)
        {
            // TODO: Improve this, do not use fixed values to resolve index
            default:
            case PororocaWebSocketClientMessageContentMode.Raw:
                ContentModeSelectedIndex = 0;
                IsContentModeRawSelected = true;
                break;
            case PororocaWebSocketClientMessageContentMode.File:
                ContentModeSelectedIndex = 1;
                IsContentModeFileSelected = true;
                break;
        }

        // we need to always set RawContent with a value, even if it is null,
        // to initialize with a TextDocument object
        RawContent = msg.RawContent;

        switch (msg.RawContentSyntax)
        {
            // TODO: Improve this, do not use fixed values to resolve index
            default:
            case PororocaWebSocketMessageRawContentSyntax.Json:
                RawContentSyntaxSelectedIndex = 0;
                IsRawContentJsonSyntaxSelected = true;
                break;
            case PororocaWebSocketMessageRawContentSyntax.Other:
                RawContentSyntaxSelectedIndex = 1;
                IsRawContentOtherSyntaxSelected = true;
                break;
        }

        ContentFileSrcPath = msg.FileSrcPath;
        SearchContentFileCmd = ReactiveCommand.CreateFromTask(SearchContentFileAsync);

        #endregion
    }

    #region COLLECTION ORGANIZATION

    protected override void CopyThis() =>
        CollectionsGroupDataCtx.PushToCopy(ToWebSocketClientMessage());

    public PororocaWebSocketClientMessage ToWebSocketClientMessage()
    {
        PororocaWebSocketClientMessage wsCliMsg = new(
            msgType: MessageType,
            name: Name,
            contentMode: ContentMode,
            rawContent: RawContent,
            rawContentSyntax: RawContentSyntax,
            fileSrcPath: ContentFileSrcPath,
            disableCompressionForThis: DisableCompressionForThisMessage);
        
        wsCliMsg.Id = wsCliMsgId;
        return wsCliMsg;
    }

    #endregion


    #region OTHERS

    private async Task SearchContentFileAsync()
    {
        string? fileSrcPath = await SearchFileWithDialogAsync();
        if (fileSrcPath != null)
        {
            ContentFileSrcPath = fileSrcPath;
        }
    }

    private static Task<string?> SearchFileWithDialogAsync() =>
        FileExporterImporter.SelectFileFromStorageAsync();

    #endregion

}