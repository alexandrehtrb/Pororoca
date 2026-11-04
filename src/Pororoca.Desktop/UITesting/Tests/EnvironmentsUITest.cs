using System.Collections.ObjectModel;
using Avalonia.Controls;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.ViewModels;
using Pororoca.Desktop.ViewModels.DataGrids;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.VariableResolution;
using static Pororoca.Domain.Features.VariableResolution.IPororocaVariableResolver;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class EnvironmentsUITest : PororocaUITest
{
    private static readonly ObservableCollection<VariableViewModel> colVars = GenerateCollectionVariables();
    private static readonly ObservableCollection<VariableViewModel> env1Vars = GenerateEnvironmentVariables("ENV1");
    private static readonly ObservableCollection<VariableViewModel> env2Vars = GenerateEnvironmentVariables("ENV2");

    private Control RootView { get; }
    private TopMenuRobot TopMenuRobot { get; }
    private ItemsTreeRobot TreeRobot { get; }
    private CollectionRobot ColRobot { get; }
    private CollectionVariablesRobot ColVarsRobot { get; }
    private EnvironmentRobot EnvRobot { get; }
    private HttpRequestRobot HttpRobot { get; }

    public EnvironmentsUITest()
    {
        RootView = (Control)MainWindow.Instance!.Content!;
        TopMenuRobot = new(RootView);
        TreeRobot = new(RootView.FindControl<CollectionsGroupView>("mainWindowCollectionsGroup")!);
        ColRobot = new(RootView.FindControl<CollectionView>("collectionView")!);
        ColVarsRobot = new(RootView.FindControl<CollectionVariablesView>("collectionVariablesView")!);
        EnvRobot = new(RootView.FindControl<EnvironmentView>("environmentView")!);
        HttpRobot = new(RootView.FindControl<HttpRequestView>("httpReqView")!);
    }

    public override async Task RunAsync(CancellationToken cancellationToken)
    {
        await TopMenuRobot.CreateNewCollection();
        await ColRobot.Name.Edit("COL1");
        var col = (IPororocaVariableResolver)(await TreeRobot.Select("COL1"));
        var egvm = (EnvironmentsGroupViewModel)(await TreeRobot.Select("COL1/ENVS"));

        await TreeRobot.Select("COL1/VARS");
        await ColVarsRobot.SetVariables(colVars);

        // Testing effective variables without any environment
        var effectiveVars = col.GetEffectiveVariables();
        AssertCondition("COLValueA" == ReplaceTemplates("{{VarA}}", effectiveVars));
        AssertCondition("{{VarB}}" == ReplaceTemplates("{{VarB}}", effectiveVars));
        AssertCondition("COLValueC" == ReplaceTemplates("{{VarC}}", effectiveVars));
        AssertCondition("{{VarD}}" == ReplaceTemplates("{{VarD}}", effectiveVars));
        AssertCondition("Environments" == egvm.SelectedEnvironmentName);
        AssertCondition(egvm.Items.Count == 0);

        // Testing effective variables with ENV1 as current environment
        await TreeRobot.Select("COL1");
        await ColRobot.AddEnvironment.ClickOn();
        await EnvRobot.Name.Edit("ENV1");
        await EnvRobot.SetVariables(env1Vars);
        await EnvRobot.SetAsCurrentEnvironment.ClickOn();

        effectiveVars = col.GetEffectiveVariables();
        AssertCondition("ENV1ValueA" == ReplaceTemplates("{{VarA}}", effectiveVars));
        AssertCondition("{{VarB}}" == ReplaceTemplates("{{VarB}}", effectiveVars));
        AssertCondition("COLValueC" == ReplaceTemplates("{{VarC}}", effectiveVars));
        AssertCondition("ENV1ValueD" == ReplaceTemplates("{{VarD}}", effectiveVars));
        AssertCondition("Environments  (ENV1)" == egvm.SelectedEnvironmentName);
        AssertCondition(egvm.Items.Count == 1);

        // Testing effective variables with ENV2 as current environment
        await TreeRobot.Select("COL1");
        await ColRobot.AddEnvironment.ClickOn();
        await EnvRobot.Name.Edit("ENV2");
        await EnvRobot.SetVariables(env2Vars);
        await EnvRobot.SetAsCurrentEnvironment.ClickOn();

        effectiveVars = col.GetEffectiveVariables();
        AssertCondition("ENV2ValueA" == ReplaceTemplates("{{VarA}}", effectiveVars));
        AssertCondition("{{VarB}}" == ReplaceTemplates("{{VarB}}", effectiveVars));
        AssertCondition("COLValueC" == ReplaceTemplates("{{VarC}}", effectiveVars));
        AssertCondition("ENV2ValueD" == ReplaceTemplates("{{VarD}}", effectiveVars));
        AssertCondition("Environments  (ENV2)" == egvm.SelectedEnvironmentName);
        AssertCondition(egvm.Items.Count == 2);

        // Disabling ENV2 as current environment. Should return back to collection variables values.
        await EnvRobot.SetAsCurrentEnvironment.ClickOn();
        effectiveVars = col.GetEffectiveVariables();
        AssertCondition("COLValueA" == ReplaceTemplates("{{VarA}}", effectiveVars));
        AssertCondition("{{VarB}}" == ReplaceTemplates("{{VarB}}", effectiveVars));
        AssertCondition("COLValueC" == ReplaceTemplates("{{VarC}}", effectiveVars));
        AssertCondition("{{VarD}}" == ReplaceTemplates("{{VarD}}", effectiveVars));
        AssertCondition("Environments" == egvm.SelectedEnvironmentName);
        AssertCondition(egvm.Items.Count == 2);
    }

    private static ObservableCollection<VariableViewModel> GenerateCollectionVariables()
    {
        ObservableCollection<VariableViewModel> parent = new();
        parent.Add(new(parent, new(true, "VarA", "COLValueA", false)));
        parent.Add(new(parent, new(false,"VarB", "COLValueB", false)));
        parent.Add(new(parent, new(true, "VarC", "COLValueC", false)));
        return parent;
    }

    private static ObservableCollection<VariableViewModel> GenerateEnvironmentVariables(string envName)
    {
        ObservableCollection<VariableViewModel> parent = new();
        parent.Add(new(parent, new(true, "VarA", $"{envName}ValueA", false)));
        parent.Add(new(parent, new(false,"VarC", $"{envName}ValueC", false)));
        parent.Add(new(parent, new(true, "VarD", $"{envName}ValueD", false)));
        return parent;
    }
}