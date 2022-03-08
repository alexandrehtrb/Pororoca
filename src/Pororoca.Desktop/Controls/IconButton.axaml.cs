using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;

namespace Pororoca.Desktop.Controls
{
    public partial class IconButton : UserControl
    {
        public static readonly StyledProperty<IBitmap> IconProperty =
            AvaloniaProperty.Register<IconButton, IBitmap>(nameof(Icon));

        public IBitmap Icon
        {
            get { return GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<IconButton, string>(nameof(Text));

        public string Text
        {
            get { return GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly StyledProperty<ICommand> CommandProperty =
            AvaloniaProperty.Register<IconButton, ICommand>(nameof(Command));

        public ICommand Command
        {
            get { return GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public IconButton()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}