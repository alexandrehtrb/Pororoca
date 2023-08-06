using System.Reactive;
using AvaloniaEdit.Document;
using Pororoca.Desktop.ExportImport;
using Pororoca.Desktop.HotKeys;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Pororoca.Desktop.ViewModels;

public sealed class WebSocketClientMessageViewModel : CollectionOrganizationItemViewModel
{
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
        #region WEBSOCKET REQUEST MESSAGE

        DisableCompressionForThisMessage = msg.DisableCompressionForThis;
        MessageTypeSelectedIndex = msg.MessageType switch
        {
            PororocaWebSocketMessageType.Text => 0,
            PororocaWebSocketMessageType.Binary => 1,
            PororocaWebSocketMessageType.Close => 2,
            // TODO: Improve this, do not use fixed values to resolve index
            _ => 0,
        };
        ContentModeSelectedIndex = msg.ContentMode switch
        {
            PororocaWebSocketClientMessageContentMode.Raw => 0,
            PororocaWebSocketClientMessageContentMode.File => 1,
            // TODO: Improve this, do not use fixed values to resolve index
            _ => 0,
        };

        // we need to always set RawContent with a value, even if it is null,
        // to initialize with a TextDocument object
        RawContent = msg.RawContent;

        RawContentSyntaxSelectedIndex = msg.RawContentSyntax switch
        {
            PororocaWebSocketMessageRawContentSyntax.Json => 0,
            PororocaWebSocketMessageRawContentSyntax.Other => 1,
            // TODO: Improve this, do not use fixed values to resolve index
            _ => 0,
        };
        ContentFileSrcPath = msg.FileSrcPath;
        SearchContentFileCmd = ReactiveCommand.CreateFromTask(SearchContentFileAsync);

        #endregion
    }

    #region COLLECTION ORGANIZATION

    protected override void CopyThis() =>
        ClipboardArea.Instance.PushToCopy(ToWebSocketClientMessage());

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