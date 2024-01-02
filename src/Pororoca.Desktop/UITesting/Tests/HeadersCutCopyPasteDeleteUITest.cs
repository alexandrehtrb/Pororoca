using Avalonia.Controls;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class HeadersCutCopyPasteDeleteUITest : UITest
{
    private Control RootView { get; }
    private TopMenuRobot TopMenuRobot { get; }
    private ItemsTreeRobot TreeRobot { get; }
    private CollectionRobot ColRobot { get; }
    private HttpRequestRobot HttpRobot { get; }

    public HeadersCutCopyPasteDeleteUITest()
    {
        RootView = (Control)MainWindow.Instance!.Content!;
        TopMenuRobot = new(RootView);
        TreeRobot = new(RootView.FindControl<CollectionsGroupView>("mainWindowCollectionsGroup")!);
        ColRobot = new(RootView.FindControl<CollectionView>("collectionView")!);
        HttpRobot = new(RootView.FindControl<HttpRequestView>("httpReqView")!);
    }

    public override async Task RunAsync()
    {
        PororocaKeyValueParam[] headers;
        await TopMenuRobot.CreateNewCollection();
        await ColRobot.Name.Edit("COL1");

        await TreeRobot.Select("COL1");
        await ColRobot.AddHttpReq.ClickOn();
        await HttpRobot.Name.Edit("HTTP1");

        await HttpRobot.AddReqHeader.ClickOn();
        await HttpRobot.AddReqHeader.ClickOn();
        await HttpRobot.EditRequestHeaderAt(0, true, "k1", "v1");
        await HttpRobot.EditRequestHeaderAt(1, true, "k2", "v2");

        await TreeRobot.Select("COL1");
        await ColRobot.AddHttpReq.ClickOn();
        await HttpRobot.Name.Edit("HTTP2");

        await HttpRobot.AddReqHeader.ClickOn();
        await HttpRobot.AddReqHeader.ClickOn();
        await HttpRobot.EditRequestHeaderAt(0, false, "k3", "v3");
        await HttpRobot.EditRequestHeaderAt(1, true, "k4", "v4");

        // copy and paste
        await HttpRobot.SelectRequestHeaders(HttpRobot.ReqHeadersVm.Items.ToArray());
        await HttpRobot.CopySelectedRequestHeaders();
        await TreeRobot.Select("COL1/HTTP1");
        await HttpRobot.PasteRequestHeaders();

        headers = HttpRobot.ReqHeadersVm.Items.Select(x => x.ToKeyValueParam()).ToArray();
        Assert(headers.Length == 4);
        Assert(headers.Contains(new(true, "k1", "v1")));
        Assert(headers.Contains(new(true, "k2", "v2")));
        Assert(headers.Contains(new(false, "k3", "v3")));
        Assert(headers.Contains(new(true, "k4", "v4")));

        // cut and paste
        await HttpRobot.SelectRequestHeaders(HttpRobot.ReqHeadersVm.Items[0], HttpRobot.ReqHeadersVm.Items[1]);
        await HttpRobot.CutSelectedRequestHeaders();

        headers = HttpRobot.ReqHeadersVm.Items.Select(x => x.ToKeyValueParam()).ToArray();
        Assert(headers.Length == 2);
        Assert(headers.Contains(new(false, "k3", "v3")));
        Assert(headers.Contains(new(true, "k4", "v4")));

        await TreeRobot.Select("COL1/HTTP2");
        await HttpRobot.PasteRequestHeaders();

        headers = HttpRobot.ReqHeadersVm.Items.Select(x => x.ToKeyValueParam()).ToArray();
        Assert(headers.Length == 4);
        Assert(headers.Contains(new(false, "k3", "v3")));
        Assert(headers.Contains(new(true, "k4", "v4")));
        Assert(headers.Contains(new(true, "k1", "v1")));
        Assert(headers.Contains(new(true, "k2", "v2")));

        // delete
        await HttpRobot.SelectRequestHeaders(HttpRobot.ReqHeadersVm.Items[0], HttpRobot.ReqHeadersVm.Items[1]);
        await HttpRobot.DeleteSelectedRequestHeaders();

        headers = HttpRobot.ReqHeadersVm.Items.Select(x => x.ToKeyValueParam()).ToArray();
        Assert(headers.Length == 2);
        Assert(headers.Contains(new(true, "k1", "v1")));
        Assert(headers.Contains(new(true, "k2", "v2")));
    }
}