using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Pororoca.Desktop.ViewModels;

namespace Pororoca.Desktop.Views
{
    public class EditableTextBlockView : UserControl
    {
        public EditableTextBlockView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                var vm = (EditableTextBlockViewModel)DataContext!;
                vm.EditOrApplyTxtChange();
            }
        }
    }
}