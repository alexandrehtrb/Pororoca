using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using AvaloniaEdit;
using Pororoca.Desktop.TextEditorConfig;
using Pororoca.Desktop.ViewModels;
using Pororoca.Domain.Features.Entities.Pororoca.Http;

namespace Pororoca.Desktop.Views;

public sealed class HttpRequestView : UserControl
{
    private readonly AvaloniaEdit.TextMate.TextMate.Installation httpReqRawBodyEditorTextMateInstallation;
    private string? currentHttpReqRawBodySyntaxLangId;

    private readonly AvaloniaEdit.TextMate.TextMate.Installation httpResRawBodyEditorTextMateInstallation;
    private string? currentHttpResRawBodySyntaxLangId;

    public HttpRequestView()
    {
        InitializeComponent();

        var varResolverObtainer = () => ((HttpRequestViewModel)DataContext!).col;

        var httpReqRawBodyEditor = this.FindControl<TextEditor>("teReqBodyRawContent");
        this.httpReqRawBodyEditorTextMateInstallation = TextEditorConfiguration.Setup(httpReqRawBodyEditor!, true, varResolverObtainer);

        var httpReqRawContentTypeSelector = this.FindControl<AutoCompleteBox>("acbReqBodyRawContentType")!;
        httpReqRawContentTypeSelector.SelectionChanged += OnRequestBodyRawContentTypeChanged;

        var httpResRawBodyEditor = this.FindControl<TextEditor>("ResponseBodyRawContentEditor")!;
        this.httpResRawBodyEditorTextMateInstallation = TextEditorConfiguration.Setup(httpResRawBodyEditor!, false, null);
        httpResRawBodyEditor.DocumentChanged += OnResponseRawBodyEditorDocumentChanged;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    #region ON DATA CONTEXT CHANGED

    protected override void OnDataContextChanged(EventArgs e)
    {
        LoadRequestRawContentSyntaxFromVmIfSelected();
        base.OnDataContextChanged(e);
    }

    private void LoadRequestRawContentSyntaxFromVmIfSelected()
    {
        var vm = (HttpRequestViewModel?)DataContext;
        if (vm is not null && vm.RequestBodyMode == PororocaHttpRequestBodyMode.Raw)
        {
            ApplySelectedRequestRawContentSyntax(vm.RequestRawContentType);
        }
    }


    #endregion

    #region VIEW COMPONENTS EVENTS

    public void SendOrCancelRequest(object sender, RoutedEventArgs args)
    {
        var vm = (HttpRequestViewModel)DataContext!;
        Dispatcher.UIThread.Post(async () => await vm.SendOrCancelRequestAsync());
    }

    public void OnSelectedUrlEncodedParamsChanged(object sender, SelectionChangedEventArgs e) =>
        ((HttpRequestViewModel)DataContext!).UrlEncodedParamsTableVm.UpdateSelectedItems(e);

    public void OnSelectedFormDataParamsChanged(object sender, SelectionChangedEventArgs e) =>
        ((HttpRequestViewModel)DataContext!).FormDataParamsTableVm.UpdateSelectedItems(e);

    public void OnSelectedResponseCapturesChanged(object sender, SelectionChangedEventArgs e) =>
        ((HttpRequestViewModel)DataContext!).ResCapturesTableVm.UpdateSelectedItems(e);

    public void OnRequestUrlPointerEnter(object sender, PointerEventArgs e)
    {
        var vm = (HttpRequestViewModel)DataContext!;
        vm.UpdateResolvedRequestUrlToolTip();
    }

    private void OnRequestBodyRawContentTypeChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems is not null
         && e.AddedItems.Count > 0
         && e.AddedItems[0] is string selectedContentType)
        {
            ApplySelectedRequestRawContentSyntax(selectedContentType);
        }
    }

    private void OnResponseRawBodyEditorDocumentChanged(object? sender, EventArgs e)
    {
        var vm = (HttpRequestViewModel)DataContext!;
        var resVm = vm.ResponseDataCtx;
        this.httpResRawBodyEditorTextMateInstallation.SetEditorSyntax(ref this.currentHttpResRawBodySyntaxLangId, resVm.ResponseRawContentType);
    }

    #endregion

    #region HELPERS

    private void ApplySelectedRequestRawContentSyntax(string? requestRawContentType) =>
        this.httpReqRawBodyEditorTextMateInstallation.SetEditorSyntax(ref this.currentHttpReqRawBodySyntaxLangId, requestRawContentType);

    #endregion
}