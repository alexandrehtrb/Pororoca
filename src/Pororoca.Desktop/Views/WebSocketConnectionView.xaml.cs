using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using AvaloniaEdit;
using Pororoca.Desktop.TextEditorConfig;
using Pororoca.Desktop.ViewModels;
using Pororoca.Domain.Features.Common;

namespace Pororoca.Desktop.Views;

public sealed class WebSocketConnectionView : UserControl
{
    private readonly AvaloniaEdit.TextMate.TextMate.Installation selectedExchangedMessageEditorTextMateInstallation;
    private string? currentSelectedMsgSyntaxLangId;

    public WebSocketConnectionView()
    {
        InitializeComponent();

        var selectedExchangedMessageEditor = this.FindControl<TextEditor>("SelectedExchangedMessageContentEditor");
        this.selectedExchangedMessageEditorTextMateInstallation = TextEditorConfiguration.Setup(selectedExchangedMessageEditor!, false, null);

        var exchangedMessagesList = this.FindControl<ListBox>("ExchangedMessagesList")!;
        exchangedMessagesList.SelectionChanged += OnSelectedExchangedMessageChanged;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    #region VIEW COMPONENTS EVENTS

    public void ConnectDisconnectCancel(object sender, RoutedEventArgs args)
    {
        var vm = (WebSocketConnectionViewModel)DataContext!;
        Dispatcher.UIThread.Post(async () => await vm.ConnectDisconnectCancelAsync());
    }

    public void OnSelectedSubprotocolsChanged(object sender, SelectionChangedEventArgs e) =>
        ((WebSocketConnectionViewModel)DataContext!).SubprotocolsTableVm.UpdateSelectedItems(e);

    public void OnUrlPointerEnter(object sender, PointerEventArgs e)
    {
        var vm = (WebSocketConnectionViewModel)DataContext!;
        vm.UpdateResolvedUrlToolTip();
    }

    private void OnSelectedExchangedMessageChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems is not null
         && e.AddedItems.Count > 0
         && e.AddedItems[0] is WebSocketExchangedMessageViewModel emVm)
        {
            ApplySelectedMsgContentSyntax(emVm.IsJsonTextContent);
        }
    }

    #endregion

    #region HELPERS

    private void ApplySelectedMsgContentSyntax(bool isJson)
    {
        string? contentType = isJson ? MimeTypesDetector.DefaultMimeTypeForJson : null;
        this.selectedExchangedMessageEditorTextMateInstallation.SetEditorSyntax(ref this.currentSelectedMsgSyntaxLangId, contentType);
    }

    #endregion
}