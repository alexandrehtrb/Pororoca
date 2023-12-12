using Pororoca.Desktop.Controls;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting.Robots;

public sealed class CollectionFolderRobot : BaseNamedRobot
{
    public CollectionFolderRobot(CollectionFolderView rootView) : base(rootView) { }

    internal IconButton AddFolder => GetChildView<IconButton>("btAddFolder")!;
    internal IconButton AddHttpReq => GetChildView<IconButton>("btAddHttpReq")!;
    internal IconButton AddWebSocket => GetChildView<IconButton>("btAddWebSocket")!;
}