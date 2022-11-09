using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using Pororoca.Desktop.ViewModels;
using Pororoca.Desktop.TextEditorConfig;

namespace Pororoca.Desktop.Views;

public class WebSocketClientMessageView : UserControl
{
    private readonly TextEditor rawContentTextEditor;
    private readonly AvaloniaEdit.TextMate.TextMate.Installation rawContentEditorTextMateInstallation;
    private readonly ComboBox syntaxModeCombo;
    //private readonly ComboBox syntaxThemeCombo;

    public WebSocketClientMessageView()
    {
        InitializeComponent();
        
        this.rawContentTextEditor = this.FindControl<TextEditor>("RawContentEditor");
        this.rawContentEditorTextMateInstallation = TextEditorConfiguration.Setup(this.rawContentTextEditor, true);
        this.rawContentTextEditor.Document.TextChanged += OnRawContentChanged;
        
        this.syntaxModeCombo = this.FindControl<ComboBox>("RawContentSyntaxSelector");
        this.syntaxModeCombo.SelectionChanged += OnSelectedRawSyntaxChanged;

        /*this.syntaxThemeCombo = this.FindControl<ComboBox>("RawContentThemeSelector");
        this.syntaxThemeCombo.Items = Enum.GetNames(typeof(ThemeName));
        this.syntaxThemeCombo.SelectedItem = ThemeName.DarkPlus;
        this.syntaxThemeCombo.SelectionChanged += SyntaxThemeCombo_SelectionChanged;*/
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    #region VIEW COMPONENTS EVENTS

    private void OnRawContentChanged(object? sender, EventArgs e)
    {
        var vm = (WebSocketClientMessageViewModel?)DataContext;
        if (vm is not null)
        {
            vm.RawContent = this.rawContentTextEditor.Document.Text;
        }
    }

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
            SetRawContentFromVm();
            ApplySelectedRawContentSyntaxFromVm();
        }
    }

    #endregion

    #region HELPERS

    private void SetRawContentFromVm()
    {
        var vm = (WebSocketClientMessageViewModel?)DataContext;
        if (vm is not null)
        {
            this.rawContentTextEditor.Document.Text = vm.RawContent;
        }
    }

    private void ApplySelectedRawContentSyntaxFromVm()
    {
        var vm = (WebSocketClientMessageViewModel?)DataContext;
        if (vm is not null)
        {
            string? languageId = vm.IsRawContentJsonSyntaxSelected ? "jsonc" : null;

            if (languageId is null)
            {
                this.rawContentEditorTextMateInstallation.SetGrammar(null);
            }
            else
            {
                string scopeName = TextEditorConfiguration.DefaultRegistryOptions!.GetScopeByLanguageId(languageId);            
                this.rawContentEditorTextMateInstallation.SetGrammar(scopeName);
            }
        }
    }

    #endregion
}
