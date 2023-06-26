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

    public PororocaWebSocketMessageType MessageType => MessageTypeSelectedIndex switch
    {
        0 => PororocaWebSocketMessageType.Text,
        1 => PororocaWebSocketMessageType.Binary,
        2 => PororocaWebSocketMessageType.Close,
        _ => PororocaWebSocketMessageType.Text
    };

    #endregion

    #region CONTENT

    [Reactive]
    public int ContentModeSelectedIndex { get; set; }

    public PororocaWebSocketClientMessageContentMode ContentMode => ContentModeSelectedIndex switch
    {
        0 => PororocaWebSocketClientMessageContentMode.Raw,
        1 => PororocaWebSocketClientMessageContentMode.File,
        _ => PororocaWebSocketClientMessageContentMode.Raw
    };

    [Reactive]
    public TextDocument? RawContentTextDocument { get; set; }

    public string? RawContent
    {
        get => RawContentTextDocument?.Text;
        set => RawContentTextDocument = new(value ?? string.Empty);
    }

    [Reactive]
    public int RawContentSyntaxSelectedIndex { get; set; }

    public PororocaWebSocketMessageRawContentSyntax? RawContentSyntax => RawContentSyntaxSelectedIndex switch
    {
        0 => PororocaWebSocketMessageRawContentSyntax.Json,
        1 => PororocaWebSocketMessageRawContentSyntax.Other,
        _ => PororocaWebSocketMessageRawContentSyntax.Other
    };

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
                break;
            case PororocaWebSocketMessageType.Binary:
                MessageTypeSelectedIndex = 1;
                break;
            case PororocaWebSocketMessageType.Close:
                MessageTypeSelectedIndex = 2;
                break;
        }
        switch (msg.ContentMode)
        {
            // TODO: Improve this, do not use fixed values to resolve index
            default:
            case PororocaWebSocketClientMessageContentMode.Raw:
                ContentModeSelectedIndex = 0;
                break;
            case PororocaWebSocketClientMessageContentMode.File:
                ContentModeSelectedIndex = 1;
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
                break;
            case PororocaWebSocketMessageRawContentSyntax.Other:
                RawContentSyntaxSelectedIndex = 1;
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