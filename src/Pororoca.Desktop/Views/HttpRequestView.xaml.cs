using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using Pororoca.Desktop.TextEditorConfig;
using Pororoca.Desktop.ViewModels;

namespace Pororoca.Desktop.Views;

public class HttpRequestView : UserControl
{
    private readonly TextEditor httpReqRawBodyEditor;
    private readonly AvaloniaEdit.TextMate.TextMate.Installation httpReqRawBodyEditorTextMateInstallation;
    private string? currentHttpReqRawBodySyntaxLangId;

    private readonly TextEditor httpResRawBodyEditor;
    private readonly AvaloniaEdit.TextMate.TextMate.Installation httpResRawBodyEditorTextMateInstallation;
    private string? currentHttpResRawBodySyntaxLangId;

    private readonly AutoCompleteBox httpReqRawContentTypeSelector;
    private HttpRequestViewModel? PreviousDataContext { get; set; }

    public HttpRequestView()
    {
        InitializeComponent();

        this.httpReqRawBodyEditor = this.FindControl<TextEditor>("RequestBodyRawContentEditor");
        this.httpReqRawBodyEditorTextMateInstallation = TextEditorConfiguration.Setup(this.httpReqRawBodyEditor, true);
        this.httpReqRawBodyEditor.TextChanged += OnRequestBodyRawContentChanged;

        this.httpReqRawContentTypeSelector = this.FindControl<AutoCompleteBox>("RequestBodyRawContentTypeSelector");
        this.httpReqRawContentTypeSelector.SelectionChanged += OnRequestBodyRawContentTypeChanged;

        this.httpResRawBodyEditor = this.FindControl<TextEditor>("ResponseBodyRawContentEditor");
        this.httpResRawBodyEditorTextMateInstallation = TextEditorConfiguration.Setup(this.httpResRawBodyEditor, false);
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    #region VIEW COMPONENTS EVENTS

    public void OnRequestUrlPointerEnter(object sender, PointerEventArgs e)
    {
        var vm = (HttpRequestViewModel)DataContext!;
        vm.UpdateResolvedRequestUrlToolTip();
    }

    private void OnRequestBodyRawContentChanged(object? sender, EventArgs e)
    {
        var vm = (HttpRequestViewModel?)DataContext;
        if (vm is not null)
        {
            vm.RequestRawContent = this.httpReqRawBodyEditor.Document.Text;
        }
    }

    private void OnRequestBodyRawContentTypeChanged(object? sender, SelectionChangedEventArgs e)
    {
        string? selectedContentType = (string?)this.httpReqRawContentTypeSelector.SelectedItem;
        this.httpReqRawBodyEditorTextMateInstallation.SetEditorSyntax(ref this.currentHttpReqRawBodySyntaxLangId, selectedContentType);
    }

    #endregion

    #region ON DATA CONTEXT CHANGED

    protected override void OnDataContextChanged(EventArgs e)
    {
        StopListeningForResponsesOfPreviousVm();
        StartListeningForResponsesOfCurrentVm();
        LoadRequestRawBodyFromVmIfSelected();
        LoadResponseBodyFromVm();
        SetCurrentVmAsPrevious();
        base.OnDataContextChanged(e);
    }    

    private void StopListeningForResponsesOfPreviousVm()
    {
        var previousVm = PreviousDataContext;
        if (previousVm is not null)
            previousVm.ResponseDataCtx.HttpResponseBodyChanged -= OnResponseBodyChanged;        
    }

    private void StartListeningForResponsesOfCurrentVm()
    {
        var vm = (HttpRequestViewModel?)DataContext;
        if (vm is not null)
            vm.ResponseDataCtx.HttpResponseBodyChanged += OnResponseBodyChanged;        
    }

    private void LoadRequestRawBodyFromVmIfSelected()
    {
        var vm = (HttpRequestViewModel?)DataContext;
        if (vm is not null && vm.IsRequestBodyModeRawSelected)
        {
            SetSelectedContentType(vm.RequestRawContentType ?? string.Empty);
            this.httpReqRawBodyEditor.SetEditorRawContent(vm.RequestRawContent ?? string.Empty);
            this.httpReqRawBodyEditorTextMateInstallation.SetEditorSyntax(ref this.currentHttpReqRawBodySyntaxLangId, vm.RequestRawContentType);
        }
    }

    private void LoadResponseBodyFromVm()
    {
        var vm = (HttpRequestViewModel?)DataContext;
        if (vm is not null)
            OnResponseBodyChanged(vm.ResponseDataCtx.ResponseRawContent ?? string.Empty, vm.ResponseDataCtx.ResponseRawContentType);
    }

    private void SetCurrentVmAsPrevious()
    {
        var vm = (HttpRequestViewModel?)DataContext;
        if (vm is not null)
            PreviousDataContext = vm;
    }

    private void OnResponseBodyChanged(string updatedResponseContent, string? updatedResponseContentType)
    {
        this.httpResRawBodyEditor.SetEditorRawContent(updatedResponseContent ?? string.Empty);
        this.httpResRawBodyEditorTextMateInstallation.SetEditorSyntax(ref this.currentHttpResRawBodySyntaxLangId, updatedResponseContentType);
    }

    #endregion

    #region HELPERS

    private void SetSelectedContentType(string httpReqContentType) =>
        this.httpReqRawContentTypeSelector.Text = httpReqContentType;

    #endregion
}