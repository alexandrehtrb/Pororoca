using Avalonia.Controls;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting.Robots;

public sealed class CollectionVariablesRobot : BaseRobot
{
    public CollectionVariablesRobot(CollectionVariablesView rootView) : base(rootView){}

    internal Button AddVariable => GetChildView<Button>("btAddVariable")!;
    internal DataGrid Variables => GetChildView<DataGrid>("dgVariables")!;
}