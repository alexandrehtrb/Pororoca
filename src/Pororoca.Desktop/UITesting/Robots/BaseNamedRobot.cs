using Avalonia.Controls;
using Pororoca.Desktop.Controls;

namespace Pororoca.Desktop.UITesting.Robots;

public abstract class BaseNamedRobot : BaseRobot
{
    internal EditableTextBlockRobot Name { get; }

    public BaseNamedRobot(Control rootView) : base(rootView) =>
        Name = new(GetChildView<EditableTextBlock>("etbName")!);
}