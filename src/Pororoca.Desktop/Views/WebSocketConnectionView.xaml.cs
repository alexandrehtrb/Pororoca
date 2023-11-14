using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using Pororoca.Desktop.TextEditorConfig;
using Pororoca.Desktop.ViewModels;
using Pororoca.Domain.Features.Common;
using static Pororoca.Desktop.Views.DataGridSelectionUpdater;

namespace Pororoca.Desktop.Views;

public class WebSocketConnectionView : UserControl
{
    private readonly AvaloniaEdit.TextMate.TextMate.Installation selectedExchangedMessageEditorTextMateInstallation;
    private string? currentSelectedMsgSyntaxLangId;

    public WebSocketConnectionView()
    {
        InitializeComponent();

        var selectedExchangedMessageEditor = this.FindControl<TextEditor>("SelectedExchangedMessageContentEditor");
        this.selectedExchangedMessageEditorTextMateInstallation = TextEditorConfiguration.Setup(selectedExchangedMessageEditor!, false);

        var exchangedMessagesList = this.FindControl<ListBox>("ExchangedMessagesList")!;
        exchangedMessagesList.SelectionChanged += OnSelectedExchangedMessageChanged;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    #region VIEW COMPONENTS EVENTS

    public void OnSelectedConnectionRequestHeadersChanged(object sender, SelectionChangedEventArgs e)
    {
        var tableVm = ((WebSocketConnectionViewModel)DataContext!).RequestHeadersTableVm;
        UpdateVmSelectedItems(tableVm, e);
    }

    public void OnSelectedSubprotocolsChanged(object sender, SelectionChangedEventArgs e)
    {
        var tableVm = ((WebSocketConnectionViewModel)DataContext!).SubprotocolsTableVm;
        UpdateVmSelectedItems(tableVm, e);
    }

    public void OnSelectedConnectionResponseHeadersChanged(object sender, SelectionChangedEventArgs e)
    {
        var tableVm = ((WebSocketConnectionViewModel)DataContext!).ConnectionResponseHeadersTableVm;
        UpdateVmSelectedItems(tableVm, e);
    }

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