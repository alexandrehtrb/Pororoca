using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Pororoca.Desktop.Controls;

public partial class IconButton : UserControl
{
    public static readonly StyledProperty<Geometry> IconProperty =
        AvaloniaProperty.Register<IconButton, Geometry>(nameof(Icon));

    public Geometry Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<IconButton, string>(nameof(Text));

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly StyledProperty<ICommand> CommandProperty =
        AvaloniaProperty.Register<IconButton, ICommand>(nameof(Command));

    public ICommand Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public IconButton() => InitializeComponent();

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}