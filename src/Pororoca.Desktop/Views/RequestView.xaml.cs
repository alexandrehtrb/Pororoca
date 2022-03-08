using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Pororoca.Desktop.ViewModels;

namespace Pororoca.Desktop.Views
{
    public class RequestView : UserControl
    {
        public RequestView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnRequestUrlPointerEnter(object sender, PointerEventArgs e)
        {
            RequestViewModel vm = (RequestViewModel)DataContext!;
            vm.UpdateResolvedRequestUrlToolTip();
        }
    }
}