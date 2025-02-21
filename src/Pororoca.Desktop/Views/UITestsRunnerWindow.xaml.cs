using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Pororoca.Desktop.ViewModels;

namespace Pororoca.Desktop.Views;

public partial class UITestsRunnerWindow : Window
{
    public UITestsRunnerWindow()
    {
        AvaloniaXamlLoader.Load(this);
        DataContext = UITestsRunnerWindowViewModel.Instance;
        UITestsRunnerWindowViewModel.Instance.CheckIfTestFilesFolderExist();
    }

    public void RunTests(object sender, RoutedEventArgs args)
    {
        Close();
        UITestsRunnerWindowViewModel.Instance.RunTests();
    }
}