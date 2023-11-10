using Avalonia.Controls;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Entities.Pororoca.Http;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class FormDataParamsCutCopyPasteDeleteUITest : UITest
{
    private Control RootView { get; }
    private TopMenuRobot TopMenuRobot { get; }
    private ItemsTreeRobot TreeRobot { get; }
    private CollectionRobot ColRobot { get; }
    private HttpRequestRobot HttpRobot { get; }

    public FormDataParamsCutCopyPasteDeleteUITest()
    {
        RootView = (Control)MainWindow.Instance!.Content!;
        TopMenuRobot = new(RootView);
        TreeRobot = new(RootView.FindControl<CollectionsGroupView>("mainWindowCollectionsGroup")!);
        ColRobot = new(RootView.FindControl<CollectionView>("collectionView")!);
        HttpRobot = new(RootView.FindControl<HttpRequestView>("httpReqView")!);
    }

    public override async Task RunAsync()
    {
        PororocaHttpRequestFormDataParam[] fps;

        var p1 = PororocaHttpRequestFormDataParam.MakeTextParam(true, "k1", "v1", "text/plain");
        var p2 = PororocaHttpRequestFormDataParam.MakeFileParam(true, "k2", "v2.jpg", "image/jpeg");
        var p3 = PororocaHttpRequestFormDataParam.MakeTextParam(true, "k3", "v3", "text/plain");
        var p4 = PororocaHttpRequestFormDataParam.MakeFileParam(true, "k4", "v4.ppt", "application/mspowerpoint");

        await TopMenuRobot.CreateNewCollection();
        await ColRobot.Name.Edit("COL1");

        await TreeRobot.Select("COL1");
        await ColRobot.AddHttpReq.ClickOn();
        await HttpRobot.Name.Edit("HTTP1");
        await HttpRobot.SetFormDataBody(new[] { p1, p2 });

        await TreeRobot.Select("COL1");
        await ColRobot.AddHttpReq.ClickOn();
        await HttpRobot.Name.Edit("HTTP2");
        await HttpRobot.SetFormDataBody(new[] { p3, p4 });

        // copy and paste
        await HttpRobot.SelectFormDataParams(0, 1);
        await HttpRobot.CopySelectedFormDataParams();
        await TreeRobot.Select("COL1/HTTP1");
        await HttpRobot.PasteFormDataParams();

        fps = HttpRobot.FormDataParamsVm.Items.Select(x => x.ToFormDataParam()).ToArray();
        Assert(fps.Length == 4);
        Assert(fps.Contains(p1));
        Assert(fps.Contains(p2));
        Assert(fps.Contains(p3));
        Assert(fps.Contains(p4));

        // cut and paste
        await HttpRobot.SelectFormDataParams(0, 1);
        await HttpRobot.CutSelectedFormDataParams();

        fps = HttpRobot.FormDataParamsVm.Items.Select(x => x.ToFormDataParam()).ToArray();
        Assert(fps.Length == 2);
        Assert(fps.Contains(p3));
        Assert(fps.Contains(p4));

        await TreeRobot.Select("COL1/HTTP2");
        await HttpRobot.PasteFormDataParams();

        fps = HttpRobot.FormDataParamsVm.Items.Select(x => x.ToFormDataParam()).ToArray();
        Assert(fps.Length == 4);
        Assert(fps.Contains(p3));
        Assert(fps.Contains(p4));
        Assert(fps.Contains(p1));
        Assert(fps.Contains(p2));

        // delete
        await HttpRobot.SelectFormDataParams(0, 1);
        await HttpRobot.DeleteSelectedFormDataParams();

        fps = HttpRobot.FormDataParamsVm.Items.Select(x => x.ToFormDataParam()).ToArray();
        Assert(fps.Length == 2);
        Assert(fps.Contains(p1));
        Assert(fps.Contains(p2));
    }
}