using Avalonia.Controls;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class UrlEncodedParamsCutCopyPasteDeleteUITest : UITest
{
    private Control RootView { get; }
    private TopMenuRobot TopMenuRobot { get; }
    private ItemsTreeRobot TreeRobot { get; }
    private CollectionRobot ColRobot { get; }
    private HttpRequestRobot HttpRobot { get; }

    public UrlEncodedParamsCutCopyPasteDeleteUITest()
    {
        RootView = (Control)MainWindow.Instance!.Content!;
        TopMenuRobot = new(RootView);
        TreeRobot = new(RootView.FindControl<CollectionsGroupView>("mainWindowCollectionsGroup")!);
        ColRobot = new(RootView.FindControl<CollectionView>("collectionView")!);
        HttpRobot = new(RootView.FindControl<HttpRequestView>("httpReqView")!);
    }

    public override async Task RunAsync()
    {
        PororocaKeyValueParam[] ueps;
        await TopMenuRobot.CreateNewCollection();
        await ColRobot.Name.Edit("COL1");

        await TreeRobot.Select("COL1");
        await ColRobot.AddHttpReq.ClickOn();
        await HttpRobot.Name.Edit("HTTP1");
        
        await HttpRobot.SetUrlEncodedBody(Array.Empty<PororocaKeyValueParam>());
        await HttpRobot.ReqBodyUrlEncodedAddParam.ClickOn();
        await HttpRobot.ReqBodyUrlEncodedAddParam.ClickOn();
        await HttpRobot.EditUrlEncodedParamAt(0, true, "k1", "v1");
        await HttpRobot.EditUrlEncodedParamAt(1, true, "k2", "v2");

        await TreeRobot.Select("COL1");
        await ColRobot.AddHttpReq.ClickOn();
        await HttpRobot.Name.Edit("HTTP2");

        await HttpRobot.SetUrlEncodedBody(Array.Empty<PororocaKeyValueParam>());
        await HttpRobot.ReqBodyUrlEncodedAddParam.ClickOn();
        await HttpRobot.ReqBodyUrlEncodedAddParam.ClickOn();
        await HttpRobot.EditUrlEncodedParamAt(0, false, "k3", "v3");
        await HttpRobot.EditUrlEncodedParamAt(1, true, "k4", "v4");

        // copy and paste
        await HttpRobot.SelectUrlEncodedParams(HttpRobot.UrlEncodedParamsVm.Items.ToArray());
        await HttpRobot.CopySelectedUrlEncodedParams();
        await TreeRobot.Select("COL1/HTTP1");
        await HttpRobot.PasteUrlEncodedParams();

        ueps = HttpRobot.UrlEncodedParamsVm.Items.Select(x => x.ToKeyValueParam()).ToArray();
        Assert(ueps.Length == 4);
        Assert(ueps.Contains(new(true, "k1", "v1")));
        Assert(ueps.Contains(new(true, "k2", "v2")));
        Assert(ueps.Contains(new(false, "k3", "v3")));
        Assert(ueps.Contains(new(true, "k4", "v4")));

        // cut and paste
        await HttpRobot.SelectUrlEncodedParams(HttpRobot.UrlEncodedParamsVm.Items[0], HttpRobot.UrlEncodedParamsVm.Items[1]);
        await HttpRobot.CutSelectedUrlEncodedParams();

        ueps = HttpRobot.UrlEncodedParamsVm.Items.Select(x => x.ToKeyValueParam()).ToArray();
        Assert(ueps.Length == 2);
        Assert(ueps.Contains(new(false, "k3", "v3")));
        Assert(ueps.Contains(new(true, "k4", "v4")));

        await TreeRobot.Select("COL1/HTTP2");
        await HttpRobot.PasteUrlEncodedParams();
        
        ueps = HttpRobot.UrlEncodedParamsVm.Items.Select(x => x.ToKeyValueParam()).ToArray();
        Assert(ueps.Length == 4);
        Assert(ueps.Contains(new(false, "k3", "v3")));
        Assert(ueps.Contains(new(true, "k4", "v4")));
        Assert(ueps.Contains(new(true, "k1", "v1")));
        Assert(ueps.Contains(new(true, "k2", "v2")));

        // delete
        await HttpRobot.SelectUrlEncodedParams(HttpRobot.UrlEncodedParamsVm.Items[0], HttpRobot.UrlEncodedParamsVm.Items[1]);
        await HttpRobot.DeleteSelectedUrlEncodedParams();

        ueps = HttpRobot.UrlEncodedParamsVm.Items.Select(x => x.ToKeyValueParam()).ToArray();
        Assert(ueps.Length == 2);
        Assert(ueps.Contains(new(true, "k1", "v1")));
        Assert(ueps.Contains(new(true, "k2", "v2")));
    }
}