using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Pororoca.Desktop.Views;

public class CollectionsGroupView : UserControl
{
    public CollectionsGroupView() => InitializeComponent();

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}