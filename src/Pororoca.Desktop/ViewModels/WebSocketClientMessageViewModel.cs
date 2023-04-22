using System.Reactive;
using AvaloniaEdit.Document;
using Pororoca.Desktop.ExportImport;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

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

    [Reactive]
    public bool DisableCompressionForThisMessage { get; set; }

    #region MESSAGE TYPE

    [Reactive]
    public int MessageTypeSelectedIndex { get; set; }

    [Reactive]
    public bool IsMessageTypeTextSelected { get; set; }

    [Reactive]
    public bool IsMessageTypeBinarySelected { get; set; }

    [Reactive]
    public bool IsMessageTypeCloseSelected { get; set; }

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

    [Reactive]
    public int ContentModeSelectedIndex { get; set; }

    [Reactive]
    public bool IsContentModeRawSelected { get; set; }

    [Reactive]
    public bool IsContentModeFileSelected { get; set; }

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

    [Reactive]
    public TextDocument? RawContentTextDocument { get; set; }

    public string? RawContent
    {
        get => RawContentTextDocument?.Text;
        set => RawContentTextDocument = new(value ?? string.Empty);
    }

    [Reactive]
    public int RawContentSyntaxSelectedIndex { get; set; }

    [Reactive]
    public bool IsRawContentJsonSyntaxSelected { get; set; }

    [Reactive]
    public bool IsRawContentOtherSyntaxSelected { get; set; }

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

    [Reactive]
    public string? ContentFileSrcPath { get; set; }

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