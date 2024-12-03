using System.Reactive;
using AvaloniaEdit.Document;
using Pororoca.Desktop.Converters;
using Pororoca.Desktop.ExportImport;
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

    public PororocaWebSocketMessageType MessageType =>
        WebSocketMessageTypeMapping.MapIndexToEnum(MessageTypeSelectedIndex);

    #endregion

    #region CONTENT

    [Reactive]
    public int ContentModeSelectedIndex { get; set; }

    public PororocaWebSocketClientMessageContentMode ContentMode =>
        WebSocketClientMessageContentModeMapping.MapIndexToEnum(ContentModeSelectedIndex);

    [Reactive]
    public TextDocument? RawContentTextDocument { get; set; }

    public string? RawContent
    {
        get => RawContentTextDocument?.Text;
        set => RawContentTextDocument = new(value ?? string.Empty);
    }

    [Reactive]
    public int RawContentSyntaxSelectedIndex { get; set; }

    public PororocaWebSocketMessageRawContentSyntax? RawContentSyntax =>
        WebSocketMessageRawContentSyntaxMapping.MapIndexToEnum(RawContentSyntaxSelectedIndex);

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
        MessageTypeSelectedIndex = WebSocketMessageTypeMapping.MapEnumToIndex(msg.MessageType);
        ContentModeSelectedIndex = WebSocketClientMessageContentModeMapping.MapEnumToIndex(msg.ContentMode);

        // we need to always set RawContent with a value, even if it is null,
        // to initialize with a TextDocument object
        RawContent = msg.RawContent;
        RawContentSyntaxSelectedIndex = WebSocketMessageRawContentSyntaxMapping.MapEnumToIndex(msg.RawContentSyntax);
        ContentFileSrcPath = msg.FileSrcPath;
        SearchContentFileCmd = ReactiveCommand.CreateFromTask(SearchContentFileAsync);

        #endregion
    }

    #region COLLECTION ORGANIZATION

    public PororocaWebSocketClientMessage ToWebSocketClientMessage()
    {
        PororocaWebSocketClientMessage wsCliMsg = new(
            MessageType: MessageType,
            Name: Name,
            ContentMode: ContentMode,
            RawContent: ContentMode == PororocaWebSocketClientMessageContentMode.Raw ? RawContent : null,
            RawContentSyntax: ContentMode == PororocaWebSocketClientMessageContentMode.Raw ? RawContentSyntax : null,
            FileSrcPath: ContentFileSrcPath,
            DisableCompressionForThis: DisableCompressionForThisMessage);

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