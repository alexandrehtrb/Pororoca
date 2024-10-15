using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Pororoca.Desktop.ViewModels;

namespace Pororoca.Desktop.Views;

public partial class UITestsPrepareWindow : Window
{
    public UITestsPrepareWindow()
    {
        AvaloniaXamlLoader.Load(this);
        DataContext = UITestsPrepareWindowViewModel.Instance;
        UITestsPrepareWindowViewModel.Instance.CheckIfTestFilesFolderExist();
    }

    public void RunTests(object sender, RoutedEventArgs args)
    {
        Close();
        UITestsPrepareWindowViewModel.Instance.RunTests();
    }
}