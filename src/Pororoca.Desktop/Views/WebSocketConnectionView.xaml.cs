using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using Pororoca.Desktop.TextEditorConfig;
using Pororoca.Desktop.ViewModels;
using Pororoca.Domain.Features.Common;

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

        SetupSelectedOptionsPanelsVisibility();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    #region VIEW COMPONENTS EVENTS

    private void SetupSelectedOptionsPanelsVisibility()
    {
        ComboBox cbWsScrReqOptions = this.FindControl<ComboBox>("cbWsScrReqOptions")!;

        ComboBoxItem cbiWsScrReqOptionHeaders = this.FindControl<ComboBoxItem>("cbiWsScrReqOptionHeaders")!,
            cbiWsScrReqOptionSubprotocols = this.FindControl<ComboBoxItem>("cbiWsScrReqOptionSubprotocols")!,
            cbiWsScrReqOptionCompression = this.FindControl<ComboBoxItem>("cbiWsScrReqOptionCompression")!;

        Grid grWsConnReqHeaders = this.FindControl<Grid>("grWsConnReqHeaders")!,
             grWsConnReqSubprotocols = this.FindControl<Grid>("grWsConnReqSubprotocols")!;

        var spWsConnReqCompression = this.FindControl<StackPanel>("spWsConnReqCompression")!;

        cbWsScrReqOptions.SelectionChanged += (sender, e) =>
        {
            object? selected = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;
            if (selected == cbiWsScrReqOptionHeaders)
            {
                grWsConnReqHeaders.IsVisible = true;
                grWsConnReqSubprotocols.IsVisible = spWsConnReqCompression.IsVisible = false;
            }
            else if (selected == cbiWsScrReqOptionSubprotocols)
            {
                grWsConnReqSubprotocols.IsVisible = true;
                grWsConnReqHeaders.IsVisible = spWsConnReqCompression.IsVisible = false;
            }
            else if (selected == cbiWsScrReqOptionCompression)
            {
                spWsConnReqCompression.IsVisible = true;
                grWsConnReqHeaders.IsVisible = grWsConnReqSubprotocols.IsVisible = false;
            }
        };
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