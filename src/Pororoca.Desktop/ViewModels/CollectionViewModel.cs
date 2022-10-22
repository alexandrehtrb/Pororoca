using System.Collections.ObjectModel;
using System.Reactive;
using System.Text;
using Avalonia.Controls;
using Pororoca.Desktop.Localization;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.ExportCollection;
using Pororoca.Domain.Features.VariableResolution;
using ReactiveUI;
using static Pororoca.Domain.Features.Common.AvailablePororocaRequestSelectionOptions;
using static Pororoca.Domain.Features.VariableResolution.IPororocaVariableResolver;

namespace Pororoca.Desktop.ViewModels;

public sealed class CollectionViewModel : CollectionOrganizationItemParentViewModel<CollectionOrganizationItemViewModel>, IPororocaVariableResolver
{
    #region COLLECTION ORGANIZATION

    public override Action OnAfterItemDeleted => Parent.OnAfterItemDeleted;
    public override Action<CollectionOrganizationItemViewModel> OnRenameSubItemSelected => Parent.OnRenameSubItemSelected;
    public ReactiveCommand<Unit, Unit> MoveUpCmd { get; }
    public ReactiveCommand<Unit, Unit> MoveDownCmd { get; }
    public ReactiveCommand<Unit, Unit> AddNewFolderCmd { get; }
    public ReactiveCommand<Unit, Unit> AddNewHttpRequestCmd { get; }
    public ReactiveCommand<Unit, Unit> AddNewWebSocketConnectionCmd { get; }
    public ReactiveCommand<Unit, Unit> AddNewEnvironmentCmd { get; }
    public ReactiveCommand<Unit, Unit> DuplicateCollectionCmd { get; }
    public ReactiveCommand<Unit, Unit> PasteToCollectionCmd { get; }
    public ReactiveCommand<Unit, Unit> RenameCollectionCmd { get; }
    public ReactiveCommand<Unit, Unit> DeleteCollectionCmd { get; }
    public ReactiveCommand<Unit, Unit> ImportEnvironmentsCmd { get; }
    public ReactiveCommand<Unit, Unit> ExportCollectionCmd { get; }
    public ReactiveCommand<Unit, Unit> ExportAsPororocaCollectionCmd { get; }
    public ReactiveCommand<Unit, Unit> ExportAsPostmanCollectionCmd { get; }

    #endregion

    #region COLLECTION

    private readonly Guid colId;
    private readonly DateTimeOffset colCreatedAt;
    private bool includeSecretVariablesField;
    public bool IncludeSecretVariables
    {
        get => this.includeSecretVariablesField;
        set => this.RaiseAndSetIfChanged(ref this.includeSecretVariablesField, value);
    }
    public override ObservableCollection<CollectionOrganizationItemViewModel> Items { get; }

    #endregion

    #region OTHERS

    private bool isOperatingSystemMacOsxField;
    public bool IsOperatingSystemMacOsx
    {
        get => this.isOperatingSystemMacOsxField;
        set => this.RaiseAndSetIfChanged(ref this.isOperatingSystemMacOsxField, value);
    }

    #endregion

    public CollectionViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                               PororocaCollection col,
                               Action<CollectionViewModel> onDuplicateCollectionSelected,
                               Func<bool>? isOperatingSystemMacOsx = null) : base(parentVm, col.Name)
    {
        #region OTHERS
        this.isOperatingSystemMacOsxField = (isOperatingSystemMacOsx ?? OperatingSystem.IsMacOS)();
        #endregion

        #region COLLECTION ORGANIZATION

        MoveUpCmd = ReactiveCommand.Create(MoveThisUp);
        MoveDownCmd = ReactiveCommand.Create(MoveThisDown);
        AddNewFolderCmd = ReactiveCommand.Create(AddNewFolder);
        AddNewHttpRequestCmd = ReactiveCommand.Create(AddNewHttpRequest);
        AddNewWebSocketConnectionCmd = ReactiveCommand.Create(AddNewWebSocketConnection);
        AddNewEnvironmentCmd = ReactiveCommand.Create(AddNewEnvironment);
        DuplicateCollectionCmd = ReactiveCommand.Create(() => onDuplicateCollectionSelected(this));
        PasteToCollectionCmd = ReactiveCommand.Create(PasteToThis);
        RenameCollectionCmd = ReactiveCommand.Create(RenameThis);
        DeleteCollectionCmd = ReactiveCommand.Create(Delete);
        ImportEnvironmentsCmd = ReactiveCommand.CreateFromTask(ImportEnvironmentsAsync);
        ExportCollectionCmd = ReactiveCommand.CreateFromTask(ExportCollectionAsync);
        ExportAsPororocaCollectionCmd = ReactiveCommand.CreateFromTask(ExportAsPororocaCollectionAsync);
        ExportAsPostmanCollectionCmd = ReactiveCommand.CreateFromTask(ExportAsPostmanCollectionAsync);

        #endregion

        #region COLLECTION

        this.colId = col.Id;
        this.colCreatedAt = col.CreatedAt;
        Items = new()
        {
            new CollectionVariablesViewModel(this, col),
            new EnvironmentsGroupViewModel(this, col.Environments)
        };
        foreach (var folder in col.Folders)
            Items.Add(new CollectionFolderViewModel(this, this, folder));
        foreach (var req in col.HttpRequests)
            Items.Add(new HttpRequestViewModel(this, this, req));
        foreach (var ws in col.WebSocketConnections)
            Items.Add(new WebSocketConnectionViewModel(this, this, ws));

        RefreshSubItemsAvailableMovements();

        #endregion
    }

    #region COLLECTION ORGANIZATION

    public override void RefreshSubItemsAvailableMovements()
    {
        for (int x = 0; x < Items.Count; x++)
        {
            var colItemVm = Items[x];
            bool canMoveUp = x > 2; // Variables and Environments must remain at their positions
            bool canMoveDown = x < Items.Count - 1;
            colItemVm.CanMoveUp = canMoveUp;
            colItemVm.CanMoveDown = canMoveDown;
        }
    }

    private void AddNewFolder()
    {
        PororocaCollectionFolder newFolder = new(Localizer.Instance["Folder/NewFolder"]);
        AddFolder(newFolder, showItemInScreen: true);
    }

    private void AddNewHttpRequest()
    {
        PororocaHttpRequest newReq = new(Localizer.Instance["HttpRequest/NewRequest"]);
        AddHttpRequest(newReq, showItemInScreen: true);
    }

    private void AddNewWebSocketConnection()
    {
        PororocaWebSocketConnection newWs = new(Localizer.Instance["WebSocketConnection/NewConnection"]);
        AddWebSocketConnection(newWs, showItemInScreen: true);
    }

    private void AddNewEnvironment()
    {
        var environmentsGroup = (EnvironmentsGroupViewModel)Items.First(i => i is EnvironmentsGroupViewModel);
        environmentsGroup.AddNewEnvironment();
    }

    private Task ImportEnvironmentsAsync()
    {
        var environmentsGroup = (EnvironmentsGroupViewModel)Items.First(i => i is EnvironmentsGroupViewModel);
        return environmentsGroup.ImportEnvironmentsAsync();
    }

    public void AddFolder(PororocaCollectionFolder folderToAdd, bool showItemInScreen = false)
    {
        var variablesGroup = Items.First(i => i is CollectionVariablesViewModel);
        var environmentsGroup = Items.First(i => i is EnvironmentsGroupViewModel);
        var existingFolders = Items.Where(i => i is CollectionFolderViewModel);
        var existingRequests = Items.Where(i => i is HttpRequestViewModel || i is WebSocketConnectionViewModel);
        CollectionFolderViewModel folderToAddVm = new(this, this, folderToAdd);

        var rearrangedItems = new[] { variablesGroup, environmentsGroup }
                                                                .Concat(existingFolders)
                                                                .Append(folderToAddVm)
                                                                .Concat(existingRequests)
                                                                .ToArray();
        Items.Clear();
        foreach (var item in rearrangedItems)
        {
            Items.Add(item);
        }
        IsExpanded = true;
        RefreshSubItemsAvailableMovements();
        SetAsItemInFocus(folderToAddVm, showItemInScreen);
    }

    public void AddHttpRequest(PororocaHttpRequest reqToAdd, bool showItemInScreen = false)
    {
        var variablesGroup = Items.First(i => i is CollectionVariablesViewModel);
        var environmentsGroup = Items.First(i => i is EnvironmentsGroupViewModel);
        var existingFolders = Items.Where(i => i is CollectionFolderViewModel);
        var existingRequests = Items.Where(i => i is HttpRequestViewModel || i is WebSocketConnectionViewModel);
        HttpRequestViewModel reqToAddVm = new(this, this, reqToAdd);

        var rearrangedItems = new[] { variablesGroup, environmentsGroup }
                               .Concat(existingFolders)
                               .Concat(existingRequests)
                               .Append(reqToAddVm)
                               .ToArray();

        Items.Clear();
        foreach (var item in rearrangedItems)
        {
            Items.Add(item);
        }
        IsExpanded = true;
        RefreshSubItemsAvailableMovements();
        SetAsItemInFocus(reqToAddVm, showItemInScreen);
    }

    public void AddWebSocketConnection(PororocaWebSocketConnection wsToAdd, bool showItemInScreen = false)
    {
        var variablesGroup = Items.First(i => i is CollectionVariablesViewModel);
        var environmentsGroup = Items.First(i => i is EnvironmentsGroupViewModel);
        var existingFolders = Items.Where(i => i is CollectionFolderViewModel);
        var existingRequests = Items.Where(i => i is HttpRequestViewModel || i is WebSocketConnectionViewModel);
        WebSocketConnectionViewModel wsToAddVm = new(this, this, wsToAdd);

        var rearrangedItems = new[] { variablesGroup, environmentsGroup }
                               .Concat(existingFolders)
                               .Concat(existingRequests)
                               .Append(wsToAddVm)
                               .ToArray();

        Items.Clear();
        foreach (var item in rearrangedItems)
        {
            Items.Add(item);
        }
        IsExpanded = true;
        RefreshSubItemsAvailableMovements();
        SetAsItemInFocus(wsToAddVm, showItemInScreen);
    }

    protected override void CopyThis() =>
        throw new NotImplementedException();

    public override void PasteToThis()
    {
        var itemsToPaste = CollectionsGroupDataCtx.FetchCopiesOfFoldersAndReqs();
        foreach (var itemToPaste in itemsToPaste)
        {
            if (itemToPaste is PororocaCollectionFolder folderToPaste)
                AddFolder(folderToPaste);
            else if (itemToPaste is PororocaHttpRequest httpReqToPaste)
                AddHttpRequest(httpReqToPaste);
            else if (itemToPaste is PororocaWebSocketConnection wsToPaste)
                AddWebSocketConnection(wsToPaste);
        }

        var envGpVm = (EnvironmentsGroupViewModel)Items.First(i => i is EnvironmentsGroupViewModel);
        envGpVm.PasteToThis();
    }

    #region EXPORT COLLECTION

    private Task ExportCollectionAsync()
    {
        SaveFileDialog saveFileDialog = new()
        {
            Title = Localizer.Instance["Collection/ExportCollectionDialogTitle"],
            Filters = new()
            {
                new()
                {
                    Name = Localizer.Instance["Collection/PororocaCollectionFormat"],
                    Extensions = new List<string> { PororocaCollectionExtension }
                },
                new()
                {
                    Name = Localizer.Instance["Collection/PostmanCollectionFormat"],
                    Extensions = new List<string> { PostmanCollectionExtension }
                }
            }
        };

        return ShowExportCollectionDialogAsync(saveFileDialog);
    }

    private Task ExportAsPororocaCollectionAsync()
    {
        SaveFileDialog saveFileDialog = new()
        {
            Title = Localizer.Instance["Collection/ExportAsPororocaCollectionDialogTitle"],
            InitialFileName = $"{Name}.{PororocaCollectionExtension}"
        };

        return ShowExportCollectionDialogAsync(saveFileDialog);
    }

    private Task ExportAsPostmanCollectionAsync()
    {
        SaveFileDialog saveFileDialog = new()
        {
            Title = Localizer.Instance["Collection/ExportAsPostmanCollectionDialogTitle"],
            InitialFileName = $"{Name}.{PostmanCollectionExtension}"
        };

        return ShowExportCollectionDialogAsync(saveFileDialog);
    }

    private async Task ShowExportCollectionDialogAsync(SaveFileDialog saveFileDialog)
    {
        string? saveFileOutputPath = await saveFileDialog.ShowAsync(MainWindow.Instance!);
        if (saveFileOutputPath != null
            &&
            (saveFileOutputPath.EndsWith(PororocaCollectionExtension) || saveFileOutputPath.EndsWith(PostmanCollectionExtension))
           )
        {
            var c = ToCollection();
            string json = saveFileOutputPath.EndsWith(PostmanCollectionExtension) ?
                PostmanCollectionV21Exporter.ExportAsPostmanCollectionV21(c, !IncludeSecretVariables) :
                PororocaCollectionExporter.ExportAsPororocaCollection(c, !IncludeSecretVariables);
            await File.WriteAllTextAsync(saveFileOutputPath, json, Encoding.UTF8);
        }
    }

    #endregion


    #endregion

    #region COLLECTION

    public PororocaCollection ToCollection()
    {
        PororocaCollection newCol = new(this.colId, Name, this.colCreatedAt);
        foreach (var colItemVm in Items)
        {
            if (colItemVm is CollectionVariablesViewModel colVarsVm)
                newCol.UpdateVariables(colVarsVm.ToVariables());
            else if (colItemVm is EnvironmentsGroupViewModel colEnvsVm)
                newCol.UpdateEnvironments(colEnvsVm.ToEnvironments());
            else if (colItemVm is CollectionFolderViewModel colFolderVm)
                newCol.AddFolder(colFolderVm.ToCollectionFolder());
            else if (colItemVm is HttpRequestViewModel httpReqVm)
                newCol.AddRequest(httpReqVm.ToHttpRequest());
            else if (colItemVm is WebSocketConnectionViewModel wsVm)
                newCol.AddRequest(wsVm.ToWebSocketConnection());
        }
        return newCol;
    }

    public string ReplaceTemplates(string? strToReplaceTemplatedVariables)
    {
        if (string.IsNullOrEmpty(strToReplaceTemplatedVariables))
        {
            return string.Empty;
        }
        else
        {
            var collectionVariables = ((CollectionVariablesViewModel)Items.First(i => i is CollectionVariablesViewModel)).ToVariables();
            IEnumerable<PororocaVariable>? environmentVariables = ((EnvironmentsGroupViewModel)Items.First(i => i is EnvironmentsGroupViewModel))
                                                                  .Items
                                                                  .FirstOrDefault(evm => evm.IsCurrentEnvironment)
                                                                  ?.ToEnvironment()
                                                                  ?.Variables;
            IEnumerable<PororocaVariable> effectiveVariables = PororocaVariablesMerger.MergeVariables(collectionVariables, environmentVariables);
            string resolvedStr = strToReplaceTemplatedVariables!;
            foreach (var v in effectiveVariables)
            {
                string variableTemplate = VariableTemplateBeginToken + v.Key + VariableTemplateEndToken;
                resolvedStr = resolvedStr.Replace(variableTemplate, v.Value ?? string.Empty);
            }
            return resolvedStr;
        }
    }

    #endregion
}