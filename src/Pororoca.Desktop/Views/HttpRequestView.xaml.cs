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

    public HttpRequestView()
    {
        InitializeComponent();

        DataContextChanged += WhenDataContextChanged;

        this.httpReqRawBodyEditor = this.FindControl<TextEditor>("RequestBodyRawContentEditor");
        this.httpReqRawBodyEditorTextMateInstallation = TextEditorConfiguration.Setup(this.httpReqRawBodyEditor, true);

        this.httpResRawBodyEditor = this.FindControl<TextEditor>("ResponseBodyRawContentEditor");
        this.httpResRawBodyEditorTextMateInstallation = TextEditorConfiguration.Setup(this.httpResRawBodyEditor, false);

        this.httpReqRawContentTypeSelector = this.FindControl<AutoCompleteBox>("RequestBodyRawContentTypeSelector");
        this.httpReqRawContentTypeSelector.SelectionChanged += OnRequestBodyRawContentTypeChanged;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    public void OnRequestUrlPointerEnter(object sender, PointerEventArgs e)
    {
        var vm = (HttpRequestViewModel)DataContext!;
        vm.UpdateResolvedRequestUrlToolTip();
    }

    private void WhenDataContextChanged(object? sender, EventArgs e)
    {
        // I have not found a way to work with TextEditor binding with Text,
        // hence I am using its events here
        var vm = (HttpRequestViewModel?)DataContext;
        if (vm is not null)
        {
            this.httpReqRawBodyEditor.Document.TextChanged -= OnRequestBodyRawContentChanged;
            this.httpReqRawBodyEditor.Document.TextChanged += OnRequestBodyRawContentChanged;

            vm.ResponseDataCtx.HttpResponseBodyChanged -= OnResponseBodyChanged;
            vm.ResponseDataCtx.HttpResponseBodyChanged += OnResponseBodyChanged;

            if (vm.IsRequestBodyModeRawSelected && vm.RequestRawContent is not null)
            {
                OnRequestBodyChanged(vm.RequestRawContent, vm.RequestRawContentType);
            }
            if (vm.ResponseDataCtx.ResponseRawContent is not null)
            {
                OnResponseBodyChanged(vm.ResponseDataCtx.ResponseRawContent, vm.ResponseDataCtx.ResponseRawContentType);
            }
        }
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
        string? selectedContentType = (string?) this.httpReqRawContentTypeSelector.SelectedItem;
        SetEditorSyntax(this.httpReqRawBodyEditorTextMateInstallation, selectedContentType);
    }

    private void OnRequestBodyChanged(string updatedRequestContent, string? updatedRequestContentType)
    {
        this.httpReqRawContentTypeSelector.Text = updatedRequestContentType;
        SetEditorSyntax(this.httpReqRawBodyEditorTextMateInstallation, updatedRequestContentType);
        SetEditorRawContent(this.httpReqRawBodyEditor, updatedRequestContent);
    }

    private void OnResponseBodyChanged(string updatedResponseContent, string? updatedResponseContentType)
    {
        SetEditorSyntax(this.httpResRawBodyEditorTextMateInstallation, updatedResponseContentType);
        SetEditorRawContent(this.httpResRawBodyEditor, updatedResponseContent);
    }

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
}