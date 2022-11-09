using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using Pororoca.Desktop.TextEditorConfig;
using Pororoca.Desktop.ViewModels;

namespace Pororoca.Desktop.Views;

public class WebSocketConnectionView : UserControl
{
    private readonly TextEditor selectedExchangedMessageEditor;
    private readonly AvaloniaEdit.TextMate.TextMate.Installation selectedExchangedMessageEditorTextMateInstallation;

    public WebSocketConnectionView()
    {
        InitializeComponent();

        this.selectedExchangedMessageEditor = this.FindControl<TextEditor>("SelectedExchangedMessageContentEditor");
        this.selectedExchangedMessageEditorTextMateInstallation = TextEditorConfiguration.Setup(this.selectedExchangedMessageEditor, false);
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    #region VIEW COMPONENTS EVENTS

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
            LoadSelectedMsgFromVm();
        }
    }

    #endregion

    #region ON DATA CONTEXT CHANGED

    protected override void OnDataContextChanged(EventArgs e)
    {
        LoadSelectedMsgFromVm();
        base.OnDataContextChanged(e);
    }

    private void LoadSelectedMsgFromVm()
    {
        var vm = (WebSocketConnectionViewModel?)DataContext;
        if (vm is not null)
        {
            SetSelectedMsgContentFromVm();
            ApplySelectedMsgContentSyntaxFromVm();
        }
    }

    #endregion

    #region HELPERS

    private void SetSelectedMsgContentFromVm()
    {
        var vm = (WebSocketConnectionViewModel?)DataContext;
        if (vm is not null)
        {
            this.selectedExchangedMessageEditor.Document.Text = vm.SelectedExchangedMessageContent ?? string.Empty;
        }
    }

    private void ApplySelectedMsgContentSyntaxFromVm()
    {
        var vm = (WebSocketConnectionViewModel?)DataContext;
        if (vm is not null)
        {
            string? languageId = vm.IsSelectedExchangedMessageContentJson ? "jsonc" : null;

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

    #endregion
    
}