using Avalonia.Controls;

namespace Pororoca.Desktop.UITesting.Robots;

public sealed class ItemsTreeRobot : BaseRobot
{
    public ItemsTreeRobot(TreeView rootView) : base(rootView) { }
    
    internal TreeView Tree => (TreeView)this.RootView;
}