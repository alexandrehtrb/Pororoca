using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using Pororoca.Desktop.TextEditorConfig;
using Pororoca.Desktop.ViewModels;
using Pororoca.Domain.Features.Entities.Pororoca.Http;

namespace Pororoca.Desktop.Views;

public class HttpRequestView : UserControl
{
    private readonly AvaloniaEdit.TextMate.TextMate.Installation httpReqRawBodyEditorTextMateInstallation;
    private string? currentHttpReqRawBodySyntaxLangId;

    private readonly AvaloniaEdit.TextMate.TextMate.Installation httpResRawBodyEditorTextMateInstallation;
    private string? currentHttpResRawBodySyntaxLangId;

    public HttpRequestView()
    {
        InitializeComponent();

        var httpReqRawBodyEditor = this.FindControl<TextEditor>("teReqBodyRawContent");
        this.httpReqRawBodyEditorTextMateInstallation = TextEditorConfiguration.Setup(httpReqRawBodyEditor!, true);

        var httpReqRawContentTypeSelector = this.FindControl<AutoCompleteBox>("acbReqBodyRawContentType")!;
        httpReqRawContentTypeSelector.SelectionChanged += OnRequestBodyRawContentTypeChanged;

        var httpResRawBodyEditor = this.FindControl<TextEditor>("ResponseBodyRawContentEditor")!;
        this.httpResRawBodyEditorTextMateInstallation = TextEditorConfiguration.Setup(httpResRawBodyEditor!, false);
        httpResRawBodyEditor.DocumentChanged += OnResponseRawBodyEditorDocumentChanged;

        SetupSelectedOptionsPanelsVisibility();
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

    private void SetupSelectedOptionsPanelsVisibility()
    {
        ComboBox cbReqBodyMode = this.FindControl<ComboBox>("cbReqBodyMode")!;

        ComboBoxItem cbiReqBodyModeNone = this.FindControl<ComboBoxItem>("cbiReqBodyModeNone")!,
            cbiReqBodyModeRaw = this.FindControl<ComboBoxItem>("cbiReqBodyModeRaw")!,
            cbiReqBodyModeFile = this.FindControl<ComboBoxItem>("cbiReqBodyModeFile")!,
            cbiReqBodyModeUrlEncoded = this.FindControl<ComboBoxItem>("cbiReqBodyModeUrlEncoded")!,
            cbiReqBodyModeFormData = this.FindControl<ComboBoxItem>("cbiReqBodyModeFormData")!,
            cbiReqBodyModeGraphQl = this.FindControl<ComboBoxItem>("cbiReqBodyModeGraphQl")!;

        Grid grReqBodyFile = this.FindControl<Grid>("grReqBodyFile")!,
             grReqBodyUrlEncoded = this.FindControl<Grid>("grReqBodyUrlEncoded")!,
             grReqBodyFormData = this.FindControl<Grid>("grReqBodyFormData")!,
             grReqBodyGraphQl = this.FindControl<Grid>("grReqBodyGraphQl")!;

        var acbReqBodyRawContentType = this.FindControl<AutoCompleteBox>("acbReqBodyRawContentType")!;
        var teReqBodyRawContent = this.FindControl<TextEditor>("teReqBodyRawContent")!;

        cbReqBodyMode.SelectionChanged += (sender, e) =>
        {
            object? selected = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;
            if (selected == cbiReqBodyModeNone)
            {
                acbReqBodyRawContentType.IsVisible =
                teReqBodyRawContent.IsVisible =
                grReqBodyFile.IsVisible =
                grReqBodyUrlEncoded.IsVisible =
                grReqBodyFormData.IsVisible =
                grReqBodyGraphQl.IsVisible = false;
            }
            else if (selected == cbiReqBodyModeRaw)
            {
                acbReqBodyRawContentType.IsVisible = teReqBodyRawContent.IsVisible = true;
                grReqBodyFile.IsVisible =
                grReqBodyUrlEncoded.IsVisible =
                grReqBodyFormData.IsVisible =
                grReqBodyGraphQl.IsVisible = false;
            }
            else if (selected == cbiReqBodyModeFile)
            {
                grReqBodyFile.IsVisible = true;
                acbReqBodyRawContentType.IsVisible =
                teReqBodyRawContent.IsVisible =
                grReqBodyUrlEncoded.IsVisible =
                grReqBodyFormData.IsVisible =
                grReqBodyGraphQl.IsVisible = false;
            }
            else if (selected == cbiReqBodyModeUrlEncoded)
            {
                grReqBodyUrlEncoded.IsVisible = true;
                acbReqBodyRawContentType.IsVisible =
                teReqBodyRawContent.IsVisible =
                grReqBodyFile.IsVisible =
                grReqBodyFormData.IsVisible =
                grReqBodyGraphQl.IsVisible = false;
            }
            else if (selected == cbiReqBodyModeFormData)
            {
                grReqBodyFormData.IsVisible = true;
                acbReqBodyRawContentType.IsVisible =
                teReqBodyRawContent.IsVisible =
                grReqBodyFile.IsVisible =
                grReqBodyUrlEncoded.IsVisible =
                grReqBodyGraphQl.IsVisible = false;
            }
            else if (selected == cbiReqBodyModeGraphQl)
            {
                grReqBodyGraphQl.IsVisible = true;
                acbReqBodyRawContentType.IsVisible =
                teReqBodyRawContent.IsVisible =
                grReqBodyFile.IsVisible =
                grReqBodyUrlEncoded.IsVisible =
                grReqBodyFormData.IsVisible = false;
            }
        };
    }

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