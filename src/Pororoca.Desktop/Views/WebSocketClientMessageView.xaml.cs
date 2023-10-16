using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using Pororoca.Desktop.Converters;
using Pororoca.Desktop.TextEditorConfig;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;

namespace Pororoca.Desktop.Views;

public class WebSocketClientMessageView : UserControl
{
    private readonly AvaloniaEdit.TextMate.TextMate.Installation rawContentEditorTextMateInstallation;
    private string? currentRawContentSyntaxLangId;

    public WebSocketClientMessageView()
    {
        InitializeComponent();

        var rawContentTextEditor = this.FindControl<TextEditor>("teContentRaw");
        this.rawContentEditorTextMateInstallation = TextEditorConfiguration.Setup(rawContentTextEditor!, true);

        var rawContentSyntaxSelector = this.FindControl<ComboBox>("cbContentRawSyntax")!;
        rawContentSyntaxSelector.SelectionChanged += OnRawContentSyntaxChanged;

        // This is for testing syntax colour themes
        /*this.syntaxThemeCombo = this.FindControl<ComboBox>("RawContentThemeSelector");
        this.syntaxThemeCombo.Items = Enum.GetNames(typeof(ThemeName));
        this.syntaxThemeCombo.SelectedItem = ThemeName.DarkPlus;
        this.syntaxThemeCombo.SelectionChanged += SyntaxThemeCombo_SelectionChanged;*/
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    #region VIEW COMPONENTS EVENTS

    /*private void SyntaxThemeCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        string themeNameStr = (string)this.syntaxThemeCombo.SelectedItem!;

        var theme = Enum.Parse<ThemeName>(themeNameStr);

        this.rawContentEditorTextMateInstallation.SetTheme(TextEditorConfiguration.DefaultRegistryOptions!.LoadTheme(theme));
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

    #endregion
}