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

        var rawContentTextEditor = this.FindControl<TextEditor>("teWsCliMsgContentRaw");
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
        ComboBox cbWsCliMsgContentMode = this.FindControl<ComboBox>("cbWsCliMsgContentMode")!,
            cbWsCliMsgContentRawSyntax = this.FindControl<ComboBox>("cbWsCliMsgContentRawSyntax")!;

        ComboBoxItem cbiWsCliMsgContentModeRaw = this.FindControl<ComboBoxItem>("cbiWsCliMsgContentModeRaw")!,
                    cbiWsCliMsgContentModeFile = this.FindControl<ComboBoxItem>("cbiWsCliMsgContentModeFile")!,
                    cbiWsCliMsgContentRawSyntaxJson = this.FindControl<ComboBoxItem>("cbiWsCliMsgContentRawSyntaxJson")!,
                    cbiWsCliMsgContentRawSyntaxOther = this.FindControl<ComboBoxItem>("cbiWsCliMsgContentRawSyntaxOther")!;

        var teWsCliMsgContentRaw = this.FindControl<TextEditor>("teWsCliMsgContentRaw")!;

        var spWsCliMsgContentFile = this.FindControl<StackPanel>("spWsCliMsgContentFile")!;

        cbWsCliMsgContentMode.SelectionChanged += (sender, e) =>
        {
            object? selected = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;
            if (selected == cbiWsCliMsgContentModeRaw)
            {
                cbWsCliMsgContentRawSyntax.IsVisible = teWsCliMsgContentRaw.IsVisible = true;
                spWsCliMsgContentFile.IsVisible = false;
            }
            else if (selected == cbiWsCliMsgContentModeFile)
            {
                cbWsCliMsgContentRawSyntax.IsVisible = teWsCliMsgContentRaw.IsVisible = false;
                spWsCliMsgContentFile.IsVisible = true;
            }
        };

        cbWsCliMsgContentRawSyntax.SelectionChanged += (sender, e) =>
        {
            object? selected = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;
            if (selected == cbiWsCliMsgContentRawSyntaxJson)
            {
                ApplySelectedRawContentSyntax(MimeTypesDetector.DefaultMimeTypeForJson);
            }
            else if (selected == cbiWsCliMsgContentRawSyntaxOther)
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