using Avalonia.Controls;
using Avalonia.Markup.Xaml;

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
    }
}