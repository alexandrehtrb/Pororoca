using Avalonia.Controls;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class VariablesCutCopyPasteDeleteUITest : UITest
{
    private Control RootView { get; }
    private TopMenuRobot TopMenuRobot { get; }
    private ItemsTreeRobot TreeRobot { get; }
    private CollectionRobot ColRobot { get; }
    private CollectionVariablesRobot ColVarsRobot { get; }
    private EnvironmentRobot EnvRobot { get; }

    public VariablesCutCopyPasteDeleteUITest()
    {
        RootView = (Control)MainWindow.Instance!.Content!;
        TopMenuRobot = new(RootView);
        TreeRobot = new(RootView.FindControl<CollectionsGroupView>("mainWindowCollectionsGroup")!);
        ColRobot = new(RootView.FindControl<CollectionView>("collectionView")!);
        ColVarsRobot = new(RootView.FindControl<CollectionVariablesView>("collectionVariablesView")!);
        EnvRobot = new(RootView.FindControl<EnvironmentView>("environmentView")!);
    }

    public override async Task RunAsync()
    {
        PororocaVariable[] colVars, envVars;
        await TopMenuRobot.CreateNewCollection();
        await ColRobot.Name.Edit("COL1");

        await TreeRobot.Select("COL1/VARS");
        await ColVarsRobot.AddVariable.ClickOn();
        await ColVarsRobot.AddVariable.ClickOn();
        await ColVarsRobot.AddVariable.ClickOn();
        await ColVarsRobot.EditVariableAt(0, true, "CV1", "cv1_value", false);
        await ColVarsRobot.EditVariableAt(1, false, "CV2", "cv2_value", true);
        await ColVarsRobot.EditVariableAt(2, true, "CV3", "cv3_value", true);

        await ColRobot.AddEnvironment.ClickOn();
        await EnvRobot.Name.Edit("ENV1");
        await EnvRobot.AddVariable.ClickOn();
        await EnvRobot.AddVariable.ClickOn();
        await EnvRobot.AddVariable.ClickOn();
        await EnvRobot.EditVariableAt(0, false, "EV1", "ev1_value", false);
        await EnvRobot.EditVariableAt(1, true, "EV2", "ev2_value", true);
        await EnvRobot.EditVariableAt(2, false, "EV3", "ev3_value", true);

        // copy from env to col vars
        await EnvRobot.SelectVariables(EnvRobot.VariablesVm.Items[1], EnvRobot.VariablesVm.Items[2]);
        await EnvRobot.CopySelectedVariables();
        await TreeRobot.Select("COL1/VARS");
        await ColVarsRobot.PasteVariables();

        colVars = ColVarsRobot.VariablesVm.Items.Select(x => x.ToVariable()).ToArray();
        Assert(colVars.Length == 5);
        Assert(colVars.Contains(new PororocaVariable(true, "CV1", "cv1_value", false)));
        Assert(colVars.Contains(new PororocaVariable(false, "CV2", "cv2_value", true)));
        Assert(colVars.Contains(new PororocaVariable(true, "CV3", "cv3_value", true)));
        Assert(colVars.Contains(new PororocaVariable(true, "EV2", "ev2_value", true)));
        Assert(colVars.Contains(new PororocaVariable(false, "EV3", "ev3_value", true)));

        // delete col vars
        await ColVarsRobot.SelectVariables(ColVarsRobot.VariablesVm.Items[3], ColVarsRobot.VariablesVm.Items[4]);
        await ColVarsRobot.DeleteSelectedVariables();

        colVars = ColVarsRobot.VariablesVm.Items.Select(x => x.ToVariable()).ToArray();
        Assert(colVars.Length == 3);
        Assert(colVars.Contains(new PororocaVariable(true, "CV1", "cv1_value", false)));
        Assert(colVars.Contains(new PororocaVariable(false, "CV2", "cv2_value", true)));
        Assert(colVars.Contains(new PororocaVariable(true, "CV3", "cv3_value", true)));

        // cut from col vars to env
        await ColVarsRobot.SelectVariables(ColVarsRobot.VariablesVm.Items[0], ColVarsRobot.VariablesVm.Items[2]);
        await ColVarsRobot.CutSelectedVariables();

        colVars = ColVarsRobot.VariablesVm.Items.Select(x => x.ToVariable()).ToArray();
        Assert(colVars.Length == 1);
        Assert(colVars.Contains(new PororocaVariable(false, "CV2", "cv2_value", true)));

        await TreeRobot.Select("COL1/ENVS/ENV1");
        await EnvRobot.PasteVariables();

        envVars = EnvRobot.VariablesVm.Items.Select(x => x.ToVariable()).ToArray();
        Assert(envVars.Length == 5);
        Assert(envVars.Contains(new PororocaVariable(false, "EV1", "ev1_value", false)));
        Assert(envVars.Contains(new PororocaVariable(true, "EV2", "ev2_value", true)));
        Assert(envVars.Contains(new PororocaVariable(false, "EV3", "ev3_value", true)));
        Assert(envVars.Contains(new PororocaVariable(true, "CV1", "cv1_value", false)));
        Assert(envVars.Contains(new PororocaVariable(true, "CV3", "cv3_value", true)));

        // delete env vars
        await EnvRobot.SelectVariables(EnvRobot.VariablesVm.Items.ToArray());
        await EnvRobot.DeleteSelectedVariables();
        envVars = EnvRobot.VariablesVm.Items.Select(x => x.ToVariable()).ToArray();
        Assert(envVars.Length == 0);
    }
}