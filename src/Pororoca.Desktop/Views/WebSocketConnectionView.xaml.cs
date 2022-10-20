using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Pororoca.Desktop.ViewModels;
using System.Linq;

namespace Pororoca.Desktop.Views;

public class WebSocketConnectionView : UserControl
{
    public WebSocketConnectionView() => InitializeComponent();

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
}