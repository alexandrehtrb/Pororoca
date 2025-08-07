using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using Pororoca.Desktop.TextEditorConfig;
using Pororoca.Desktop.ViewModels;
using Pororoca.Domain.Features.Common;

namespace Pororoca.Desktop.Views;

public sealed class HttpRepeaterView : UserControl
{
    private readonly AvaloniaEdit.TextMate.TextMate.Installation rawInputDataEditorTextMateInstallation;
    private string? currentRawInputDataSyntaxLangId;

    private CompletionWindow? rawInputDataCompletionWindow;

    private readonly AvaloniaEdit.TextMate.TextMate.Installation httpResRawBodyEditorTextMateInstallation;
    private string? currentHttpResRawBodySyntaxLangId;

    public HttpRepeaterView()
    {
        InitializeComponent();

        var varResolverObtainer = () => ((HttpRepeaterViewModel)DataContext!).Collection;

        var rawInputDataEditor = this.FindControl<TextEditor>("teRepetitionInputDataRaw")!;
        this.rawInputDataEditorTextMateInstallation = TextEditorConfiguration.Setup(rawInputDataEditor, true, varResolverObtainer);
        this.rawInputDataEditorTextMateInstallation.SetEditorSyntax(ref this.currentRawInputDataSyntaxLangId, MimeTypesDetector.DefaultMimeTypeForJson);
        rawInputDataEditor.TextArea.TextEntering += (sender, e) => TextEditorConfiguration.OnTextEnteringInEditorWithVariables(rawInputDataEditor, e, ref this.rawInputDataCompletionWindow);
        rawInputDataEditor.TextArea.TextEntered += (sender, e) => TextEditorConfiguration.OnTextEnteredInEditorWithVariables(rawInputDataEditor, e, varResolverObtainer, ref this.rawInputDataCompletionWindow, (sender, e) => this.rawInputDataCompletionWindow = null);

        var httpResRawBodyEditor = this.FindControl<TextEditor>("ResponseBodyRawContentEditor")!;
        this.httpResRawBodyEditorTextMateInstallation = TextEditorConfiguration.Setup(httpResRawBodyEditor!, false, null);
        httpResRawBodyEditor.DocumentChanged += OnResponseRawBodyEditorDocumentChanged;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    public void StartOrStopRepetition(object sender, RoutedEventArgs args)
    {
        var vm = (HttpRepeaterViewModel)DataContext!;
        Dispatcher.UIThread.Post(async () => await vm.StartOrStopRepetitionAsync());
    }

    private void OnResponseRawBodyEditorDocumentChanged(object? sender, EventArgs e)
    {
        var vm = (HttpRepeaterViewModel)DataContext!;
        var resVm = vm.ResponseDataCtx;
        this.httpResRawBodyEditorTextMateInstallation.SetEditorSyntax(ref this.currentHttpResRawBodySyntaxLangId, resVm.ResponseRawContentType);
    }
}