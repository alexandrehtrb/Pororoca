using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using Pororoca.Desktop.ViewModels;
using Pororoca.Desktop.TextEditorConfig;
using Pororoca.Domain.Features.Common;

namespace Pororoca.Desktop.Views;

public class WebSocketClientMessageView : UserControl
{
    private readonly AvaloniaEdit.TextMate.TextMate.Installation rawContentEditorTextMateInstallation;
    private string? currentRawContentSyntaxLangId;
    private readonly ComboBox syntaxModeCombo;
    //private readonly ComboBox syntaxThemeCombo;

    public WebSocketClientMessageView()
    {
        InitializeComponent();
        
        var rawContentTextEditor = this.FindControl<TextEditor>("RawContentEditor");
        this.rawContentEditorTextMateInstallation = TextEditorConfiguration.Setup(rawContentTextEditor!, true);
        
        this.syntaxModeCombo = this.FindControl<ComboBox>("RawContentSyntaxSelector")!;
        this.syntaxModeCombo.SelectionChanged += OnSelectedRawSyntaxChanged;

        // This is for testing syntax colour themes
        /*this.syntaxThemeCombo = this.FindControl<ComboBox>("RawContentThemeSelector");
        this.syntaxThemeCombo.Items = Enum.GetNames(typeof(ThemeName));
        this.syntaxThemeCombo.SelectedItem = ThemeName.DarkPlus;
        this.syntaxThemeCombo.SelectionChanged += SyntaxThemeCombo_SelectionChanged;*/
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    #region VIEW COMPONENTS EVENTS

    private void OnSelectedRawSyntaxChanged(object? sender, SelectionChangedEventArgs e) =>
        ApplySelectedRawContentSyntaxFromVm();

    /*private void SyntaxThemeCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        string themeNameStr = (string)this.syntaxThemeCombo.SelectedItem!;

        var theme = Enum.Parse<ThemeName>(themeNameStr);

        this.rawContentEditorTextMateInstallation.SetTheme(TextEditorConfiguration.DefaultRegistryOptions!.LoadTheme(theme));
    }*/

    #endregion

    #region ON DATA CONTEXT CHANGED

    protected override void OnDataContextChanged(EventArgs e)
    {
        LoadRawContentFromVmIfSelected();
        base.OnDataContextChanged(e);
    }

    private void LoadRawContentFromVmIfSelected()
    {
        var vm = (WebSocketClientMessageViewModel?)DataContext;
        if (vm is not null && vm.IsContentModeRawSelected)
        {
            ApplySelectedRawContentSyntaxFromVm();
        }
    }

    #endregion

    #region HELPERS

    private void ApplySelectedRawContentSyntaxFromVm()
    {
        var vm = (WebSocketClientMessageViewModel?)DataContext;
        string? contentType = vm is not null && vm.IsRawContentJsonSyntaxSelected ? MimeTypesDetector.DefaultMimeTypeForJson : null;
        this.rawContentEditorTextMateInstallation.SetEditorSyntax(ref this.currentRawContentSyntaxLangId, contentType);
    }

    #endregion
}