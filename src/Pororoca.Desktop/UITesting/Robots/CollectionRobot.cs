using Pororoca.Desktop.Controls;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting.Robots;

public sealed class CollectionRobot : BaseNamedRobot
{
    public CollectionRobot(CollectionView rootView) : base(rootView) { }

    internal IconButton AddFolder => GetChildView<IconButton>("btAddFolder")!;
    internal IconButton AddHttpReq => GetChildView<IconButton>("btAddHttpReq")!;
    internal IconButton AddWebSocket => GetChildView<IconButton>("btAddWebSocket")!;
    internal IconButton AddRepeater => GetChildView<IconButton>("btAddHttpRep")!;
    internal IconButton SetCollectionScopedReqHeaders => GetChildView<IconButton>("btSetColScopedReqHeaders")!;
    internal IconButton AddEnvironment => GetChildView<IconButton>("btAddEnvironment")!;
    internal IconButton ImportEnvironment => GetChildView<IconButton>("btImportEnv")!;
    internal IconButton ExportCollection => GetChildView<IconButton>("btExportCollection")!;
}