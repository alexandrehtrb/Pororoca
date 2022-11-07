using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using Pororoca.Desktop.TextEditorConfig;
using Pororoca.Desktop.ViewModels;
using System.Linq;
using System.Text.Json;

namespace Pororoca.Desktop.Views;

public class WebSocketConnectionView : UserControl
{
    private readonly TextEditor selectedExchangedMessageEditor;
    private readonly AvaloniaEdit.TextMate.TextMate.Installation selectedExchangedMessageEditorTextMateInstallation;

    public WebSocketConnectionView()
    {
        InitializeComponent();

        DataContextChanged += WhenDataContextChanged;

        this.selectedExchangedMessageEditor = this.FindControl<TextEditor>("SelectedExchangedMessageContentEditor");
        this.selectedExchangedMessageEditorTextMateInstallation = TextEditorConfiguration.Setup(this.selectedExchangedMessageEditor, false);
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    public void OnUrlPointerEnter(object sender, PointerEventArgs e)
    {
        var vm = (WebSocketConnectionViewModel)DataContext!;
        vm.UpdateResolvedUrlToolTip();
    }

    public void OnSelectedExchangedMessageChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0)
        {
            var connVm = (WebSocketConnectionViewModel)DataContext!;
            var emVm = (WebSocketExchangedMessageViewModel) e.AddedItems[0]!;
            connVm.UpdateSelectedExchangedMessage(emVm);
        }
    }

    private void WhenDataContextChanged(object? sender, EventArgs e)
    {
        // I have not found a way to work with TextEditor binding with Text,
        // hence I am using its events here
        var vm = (WebSocketConnectionViewModel?)DataContext;
        if (vm is not null)
        {
            vm.SelectedExchangedMessageChanged -= OnSelectedExchangedMessageChanged;
            vm.SelectedExchangedMessageChanged += OnSelectedExchangedMessageChanged;

            if (vm.SelectedExchangedMessageContent is not null)
            {
                OnSelectedExchangedMessageChanged(this.selectedExchangedMessageEditor.Document.Text,
                                                  vm.IsSelectedExchangedMessageContentJson);
            }
        }
    }

    private void OnSelectedExchangedMessageChanged(string content, bool isJson)
    {
        this.selectedExchangedMessageEditor.Document.Text = content;
        SetSelectedExchangedMessageSyntax(isJson);
    }

    private void SetSelectedExchangedMessageSyntax(bool isJson)
    {
        string? languageId = isJson ? "jsonc" : null;

        if (languageId is null)
        {
            this.selectedExchangedMessageEditorTextMateInstallation.SetGrammar(null);
        }
        else
        {
            string scopeName = TextEditorConfiguration.DefaultRegistryOptions!.GetScopeByLanguageId(languageId);            
            this.selectedExchangedMessageEditorTextMateInstallation.SetGrammar(scopeName);
        }
    }

    
}