using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using Pororoca.Desktop.Converters;
using Pororoca.Desktop.TextEditorConfig;
using Pororoca.Desktop.ViewModels;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;

namespace Pororoca.Desktop.Views;

public sealed class WebSocketClientMessageView : UserControl
{
    private readonly AvaloniaEdit.TextMate.TextMate.Installation rawContentEditorTextMateInstallation;
    private string? currentRawContentSyntaxLangId;
    //private readonly ComboBox syntaxThemeCombo;
    private CompletionWindow? rawContentCompletionWindow;

    public WebSocketClientMessageView()
    {
        InitializeComponent();

        var rawContentTextEditor = this.FindControl<TextEditor>("teContentRaw")!;
        this.rawContentEditorTextMateInstallation = TextEditorConfiguration.Setup(rawContentTextEditor, true, ProvideVariableResolver);
        rawContentTextEditor.TextArea.TextEntering += (sender, e) => TextEditorConfiguration.OnTextEnteringInEditorWithVariables(rawContentTextEditor, e, ref this.rawContentCompletionWindow);
        rawContentTextEditor.TextArea.TextEntered += (sender, e) => TextEditorConfiguration.OnTextEnteredInEditorWithVariables(rawContentTextEditor, e, ProvideVariableResolver, ref this.rawContentCompletionWindow, (sender, e) => this.rawContentCompletionWindow = null);

        var rawContentSyntaxSelector = this.FindControl<ComboBox>("cbContentRawSyntax")!;
        rawContentSyntaxSelector.SelectionChanged += OnRawContentSyntaxChanged;

        // This is for testing syntax colour themes
        /*this.syntaxThemeCombo = this.FindControl<ComboBox>("RawContentThemeSelector")!;
        this.syntaxThemeCombo.ItemsSource = Enum.GetNames(typeof(TextMateSharp.Grammars.ThemeName));
        this.syntaxThemeCombo.SelectedItem = TextMateSharp.Grammars.ThemeName.DarkPlus;
        this.syntaxThemeCombo.SelectionChanged += SyntaxThemeCombo_SelectionChanged;*/
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    #region VIEW COMPONENTS EVENTS

    /*private void SyntaxThemeCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        string themeNameStr = (string)this.syntaxThemeCombo.SelectedItem!;

        var theme = Enum.Parse<TextMateSharp.Grammars.ThemeName>(themeNameStr);

        this.rawContentEditorTextMateInstallation.SetTheme(TextEditorConfiguration.DefaultRegistryOptions!.Value.LoadTheme(theme));
    }*/

    #endregion

    #region HELPERS

    private void OnRawContentSyntaxChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems is not null && e.AddedItems.Count > 0)
        {
            int i = ((ComboBox)e.Source!).SelectedIndex;
            var selectedSyntax = WebSocketMessageRawContentSyntaxMapping.MapIndexToEnum(i);
            ApplySelectedRawContentSyntax(selectedSyntax);
        }
    }

    private void ApplySelectedRawContentSyntax(PororocaWebSocketMessageRawContentSyntax? syntax)
    {
        string? contentType = syntax switch
        {
            PororocaWebSocketMessageRawContentSyntax.Json => MimeTypesDetector.DefaultMimeTypeForJson,
            _ => null
        };
        this.rawContentEditorTextMateInstallation.SetEditorSyntax(ref this.currentRawContentSyntaxLangId, contentType);
    }

    // IMPORTANTE: este método deve retornar um CollectionViewModel,
    // e não simplesmente uma coleção, pois senão não vai atualizar
    // as variáveis de coleção e de ambiente.
    public CollectionViewModel ProvideVariableResolver() =>
        ((WebSocketConnectionViewModel)((WebSocketClientMessageViewModel)DataContext!).Parent).Collection;

    #endregion
}