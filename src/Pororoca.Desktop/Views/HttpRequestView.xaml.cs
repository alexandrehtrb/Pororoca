using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using Pororoca.Desktop.TextEditorConfig;
using Pororoca.Desktop.ViewModels;

namespace Pororoca.Desktop.Views;

public class HttpRequestView : UserControl
{
    private readonly TextEditor httpReqRawBodyEditor;
    private readonly AvaloniaEdit.TextMate.TextMate.Installation httpReqRawBodyEditorTextMateInstallation;
    private readonly TextEditor httpResRawBodyEditor;
    private readonly AvaloniaEdit.TextMate.TextMate.Installation httpResRawBodyEditorTextMateInstallation;
    private readonly AutoCompleteBox httpReqRawContentTypeSelector;
    private HttpRequestViewModel? PreviousDataContext { get; set; }

    public HttpRequestView()
    {
        InitializeComponent();

        this.httpReqRawBodyEditor = this.FindControl<TextEditor>("RequestBodyRawContentEditor");
        this.httpReqRawBodyEditorTextMateInstallation = TextEditorConfiguration.Setup(this.httpReqRawBodyEditor, true);
        this.httpReqRawBodyEditor.Document.TextChanged += OnRequestBodyRawContentChanged;

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
        SetEditorSyntax(this.httpReqRawBodyEditorTextMateInstallation, selectedContentType);
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
            this.httpReqRawContentTypeSelector.Text = vm.RequestRawContentType;
            SetEditorSyntax(this.httpReqRawBodyEditorTextMateInstallation, vm.RequestRawContentType);
            SetEditorRawContent(this.httpReqRawBodyEditor, vm.RequestRawContent ?? string.Empty);
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
        SetEditorSyntax(this.httpResRawBodyEditorTextMateInstallation, updatedResponseContentType);
        SetEditorRawContent(this.httpResRawBodyEditor, updatedResponseContent ?? string.Empty);
    }

    #endregion

    #region HELPERS

    private static void SetEditorRawContent(TextEditor editor, string updatedResponseContent)
    {
        if (updatedResponseContent != editor.Document.Text)
        {
            editor.Document.Text = updatedResponseContent;
        }
    }

    private static void SetEditorSyntax(TextMate.Installation tmInstallation, string? updatedResponseContentType)
    {
        string? languageId = FindSyntaxLanguageIdForContentType(updatedResponseContentType);

        if (languageId is null)
        {
            tmInstallation.SetGrammar(null);
        }
        else
        {
            string scopeName = TextEditorConfiguration.DefaultRegistryOptions!.GetScopeByLanguageId(languageId);
            tmInstallation.SetGrammar(scopeName);
        }
    }

    private static string? FindSyntaxLanguageIdForContentType(string? contentType)
    {
        // The list of available TextMate syntaxes can be extracted with:
        // TextEditorConfiguration.DefaultRegistryOptions!.GetAvailableLanguages()

        if (contentType is null)
            return null;
        else if (contentType.Contains("json"))
            return "jsonc";
        else if (contentType.Contains("xml"))
            return "xml";
        else if (contentType.Contains("html"))
            return "html";
        else if (contentType.Contains("javascript"))
            return "javascript";
        else if (contentType.Contains("css"))
            return "css";
        else if (contentType.Contains("markdown"))
            return "markdown";
        else if (contentType.Contains("yaml"))
            return "yaml";
        else
            return null;
    }

    #endregion
}