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

        DataContextChanged += WhenDataContextChanged;
        
        this.rawContentTextEditor = this.FindControl<TextEditor>("RawContentEditor");
        this.rawContentEditorTextMateInstallation = TextEditorConfiguration.Setup(this.rawContentTextEditor, true);
        
        this.syntaxModeCombo = this.FindControl<ComboBox>("RawContentSyntaxSelector");
        this.syntaxModeCombo.SelectionChanged += SyntaxModeCombo_SelectionChanged;

        /*this.syntaxThemeCombo = this.FindControl<ComboBox>("RawContentThemeSelector");
        this.syntaxThemeCombo.Items = Enum.GetNames(typeof(ThemeName));
        this.syntaxThemeCombo.SelectedItem = ThemeName.DarkPlus;
        this.syntaxThemeCombo.SelectionChanged += SyntaxThemeCombo_SelectionChanged;*/
    }

    private void WhenDataContextChanged(object? sender, EventArgs e)
    {
        // I have not found a way to work with TextEditor binding with Text,
        // hence I am using its events here
        var vm = (WebSocketClientMessageViewModel?)DataContext;
        if (vm is not null)
        {
            this.rawContentTextEditor.Document.TextChanged -= OnRawContentChanged;
            this.rawContentTextEditor.Document.TextChanged += OnRawContentChanged;
            if (vm.RawContent is not null)
            {
                this.rawContentTextEditor.Document.Text = vm.RawContent;
            }
        }
    }

    private void OnRawContentChanged(object? sender, EventArgs e)
    {
        var vm = (WebSocketClientMessageViewModel?)DataContext;
        if (vm is not null)
        {
            vm.RawContent = this.rawContentTextEditor.Document.Text;
        }
    }


    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    /*protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        _textMateInstallation.Dispose();
    }*/

    private void SyntaxModeCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
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

    /*private void SyntaxThemeCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        string themeNameStr = (string)this.syntaxThemeCombo.SelectedItem!;

        var theme = Enum.Parse<ThemeName>(themeNameStr);

        this.rawContentEditorTextMateInstallation.SetTheme(TextEditorConfiguration.DefaultRegistryOptions!.LoadTheme(theme));
    }*/
}
