using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using Pororoca.Desktop.TextEditorConfig;
using Pororoca.Desktop.ViewModels;
using Pororoca.Domain.Features.Common;
using static Pororoca.Desktop.Views.DataGridSelectionUpdater;

namespace Pororoca.Desktop.Views;

public sealed class HttpRepeaterView : UserControl
{
    private readonly AvaloniaEdit.TextMate.TextMate.Installation rawInputDataEditorTextMateInstallation;
    private string? currentRawInputDataSyntaxLangId;

    private readonly AvaloniaEdit.TextMate.TextMate.Installation httpResRawBodyEditorTextMateInstallation;
    private string? currentHttpResRawBodySyntaxLangId;

    public HttpRepeaterView()
    {
        InitializeComponent();

        var rawInputDataEditor = this.FindControl<TextEditor>("teRepetitionInputDataRaw");
        this.rawInputDataEditorTextMateInstallation = TextEditorConfiguration.Setup(rawInputDataEditor!, true);
        this.rawInputDataEditorTextMateInstallation.SetEditorSyntax(ref this.currentRawInputDataSyntaxLangId, MimeTypesDetector.DefaultMimeTypeForJson);

        var httpResRawBodyEditor = this.FindControl<TextEditor>("ResponseBodyRawContentEditor")!;
        this.httpResRawBodyEditorTextMateInstallation = TextEditorConfiguration.Setup(httpResRawBodyEditor!, false);
        httpResRawBodyEditor.DocumentChanged += OnResponseRawBodyEditorDocumentChanged;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    public void OnSelectedResponseHeadersAndTrailersChanged(object sender, SelectionChangedEventArgs e)
    {
        var tableVm = ((HttpRepeaterViewModel)DataContext!).ResponseDataCtx.ResponseHeadersAndTrailersTableVm;
        UpdateVmSelectedItems(tableVm, e);
    }

    private void OnResponseRawBodyEditorDocumentChanged(object? sender, EventArgs e)
    {
        var vm = (HttpRepeaterViewModel)DataContext!;
        var resVm = vm.ResponseDataCtx;
        this.httpResRawBodyEditorTextMateInstallation.SetEditorSyntax(ref this.currentHttpResRawBodySyntaxLangId, resVm.ResponseRawContentType);
    }
}