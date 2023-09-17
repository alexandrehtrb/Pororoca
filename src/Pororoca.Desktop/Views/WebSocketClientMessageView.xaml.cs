using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using Pororoca.Desktop.TextEditorConfig;
using Pororoca.Domain.Features.Common;

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

        // This is for testing syntax colour themes
        /*this.syntaxThemeCombo = this.FindControl<ComboBox>("RawContentThemeSelector");
        this.syntaxThemeCombo.Items = Enum.GetNames(typeof(ThemeName));
        this.syntaxThemeCombo.SelectedItem = ThemeName.DarkPlus;
        this.syntaxThemeCombo.SelectionChanged += SyntaxThemeCombo_SelectionChanged;*/

        SetupSelectedOptionsPanelsVisibility();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    #region PANELS VISIBILITY CONTROL

    private void SetupSelectedOptionsPanelsVisibility()
    {
        ComboBox cbContentMode = this.FindControl<ComboBox>("cbContentMode")!,
            cbContentRawSyntax = this.FindControl<ComboBox>("cbContentRawSyntax")!;

        ComboBoxItem cbiContentModeRaw = this.FindControl<ComboBoxItem>("cbiContentModeRaw")!,
                    cbiContentModeFile = this.FindControl<ComboBoxItem>("cbiContentModeFile")!,
                    cbiContentRawSyntaxJson = this.FindControl<ComboBoxItem>("cbiContentRawSyntaxJson")!,
                    cbiContentRawSyntaxOther = this.FindControl<ComboBoxItem>("cbiContentRawSyntaxOther")!;

        var teContentRaw = this.FindControl<TextEditor>("teContentRaw")!;

        var spWsCliMsgContentFile = this.FindControl<StackPanel>("spWsCliMsgContentFile")!;

        cbContentMode.SelectionChanged += (sender, e) =>
        {
            object? selected = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;
            if (selected == cbiContentModeRaw)
            {
                cbContentRawSyntax.IsVisible = teContentRaw.IsVisible = true;
                spWsCliMsgContentFile.IsVisible = false;
            }
            else if (selected == cbiContentModeFile)
            {
                cbContentRawSyntax.IsVisible = teContentRaw.IsVisible = false;
                spWsCliMsgContentFile.IsVisible = true;
            }
        };

        cbContentRawSyntax.SelectionChanged += (sender, e) =>
        {
            object? selected = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;
            if (selected == cbiContentRawSyntaxJson)
            {
                ApplySelectedRawContentSyntax(MimeTypesDetector.DefaultMimeTypeForJson);
            }
            else if (selected == cbiContentRawSyntaxOther)
            {
                ApplySelectedRawContentSyntax(null);
            }
        };
    }

    #endregion

    #region VIEW COMPONENTS EVENTS

    /*private void SyntaxThemeCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        string themeNameStr = (string)this.syntaxThemeCombo.SelectedItem!;

        var theme = Enum.Parse<ThemeName>(themeNameStr);

        this.rawContentEditorTextMateInstallation.SetTheme(TextEditorConfiguration.DefaultRegistryOptions!.LoadTheme(theme));
    }*/

    #endregion

    #region HELPERS

    private void ApplySelectedRawContentSyntax(string? contentType) =>
        this.rawContentEditorTextMateInstallation.SetEditorSyntax(ref this.currentRawContentSyntaxLangId, contentType);

    #endregion
}