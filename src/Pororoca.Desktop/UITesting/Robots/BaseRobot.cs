using Avalonia.Controls;
namespace Pororoca.Desktop.UITesting.Robots;

public abstract class BaseRobot
{
    public Control RootView { get; }

    protected BaseRobot(Control rootView) => RootView = rootView;

    protected A? GetChildView<A>(string name) where A : Control =>
        RootView.FindControl<A>(name);
}