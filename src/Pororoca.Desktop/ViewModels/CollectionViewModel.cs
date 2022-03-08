using System.Collections.ObjectModel;
using System.Reactive;
using System.Text;
using Avalonia.Controls;
using ReactiveUI;
using Pororoca.Desktop.Localization;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.ExportCollection;
using Pororoca.Domain.Features.VariableResolution;
using static Pororoca.Domain.Features.VariableResolution.IPororocaVariableResolver;
using static Pororoca.Domain.Features.Common.AvailablePororocaRequestSelectionOptions;

namespace Pororoca.Desktop.ViewModels
{
    public sealed class CollectionViewModel : CollectionOrganizationItemParentViewModel<CollectionOrganizationItemViewModel>, IPororocaVariableResolver
    {
        #region COLLECTION ORGANIZATION

        public override Action OnAfterItemDeleted => Parent.OnAfterItemDeleted;
        public override Action<CollectionOrganizationItemViewModel> OnRenameSubItemSelected => Parent.OnRenameSubItemSelected;
        public ReactiveCommand<Unit, Unit> MoveUpCmd { get; }
        public ReactiveCommand<Unit, Unit> MoveDownCmd { get; }
        public ReactiveCommand<Unit, Unit> AddNewFolderCmd { get; }
        public ReactiveCommand<Unit, Unit> AddNewRequestCmd { get; }
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

        private readonly Guid _colId;
        private readonly DateTimeOffset _colCreatedAt;         
        private bool _includeSecretVariables;
        public bool IncludeSecretVariables
        {
            get => _includeSecretVariables;
            set
            {
                this.RaiseAndSetIfChanged(ref _includeSecretVariables, value);
            }
        }
        public override ObservableCollection<CollectionOrganizationItemViewModel> Items { get; }

        #endregion

        #region OTHERS

        private bool _isOperatingSystemMacOsx;
        public bool IsOperatingSystemMacOsx
        {
            get => _isOperatingSystemMacOsx;
            set
            {
                this.RaiseAndSetIfChanged(ref _isOperatingSystemMacOsx, value);
            }
        }

        #endregion

        public CollectionViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                                   PororocaCollection col,
                                   Action<CollectionViewModel> onDuplicateCollectionSelected,
                                   Func<bool>? isOperatingSystemMacOsx = null) : base(parentVm, col.Name)
        {
            #region OTHERS
            _isOperatingSystemMacOsx = (isOperatingSystemMacOsx ?? OperatingSystem.IsMacOS)();
            #endregion

            #region COLLECTION ORGANIZATION

            MoveUpCmd = ReactiveCommand.Create(MoveThisUp);
            MoveDownCmd = ReactiveCommand.Create(MoveThisDown);
            AddNewFolderCmd = ReactiveCommand.Create(AddNewFolder);
            AddNewRequestCmd = ReactiveCommand.Create(AddNewRequest);
            AddNewEnvironmentCmd = ReactiveCommand.Create(AddNewEnvironment);
            DuplicateCollectionCmd = ReactiveCommand.Create(() => onDuplicateCollectionSelected(this));
            PasteToCollectionCmd = ReactiveCommand.Create(PasteToThis);
            RenameCollectionCmd = ReactiveCommand.Create(RenameThis);
            DeleteCollectionCmd = ReactiveCommand.Create(DeleteThis);
            ImportEnvironmentsCmd = ReactiveCommand.CreateFromTask(ImportEnvironmentsAsync);
            ExportCollectionCmd = ReactiveCommand.CreateFromTask(ExportCollectionAsync);
            ExportAsPororocaCollectionCmd = ReactiveCommand.CreateFromTask(ExportAsPororocaCollectionAsync);
            ExportAsPostmanCollectionCmd = ReactiveCommand.CreateFromTask(ExportAsPostmanCollectionAsync);

            #endregion

            #region COLLECTION

            _colId = col.Id;
            _colCreatedAt = col.CreatedAt;
            Items = new()
            {
                new CollectionVariablesViewModel(this, col),
                new EnvironmentsGroupViewModel(this, col.Environments)
            };
            foreach (PororocaCollectionFolder folder in col.Folders)
                Items.Add(new CollectionFolderViewModel(this, this, folder));
            foreach (PororocaRequest req in col.Requests)
                Items.Add(new RequestViewModel(this, this, req));

            RefreshSubItemsAvailableMovements();

            #endregion
        }

        #region COLLECTION ORGANIZATION
        
        public override void RefreshSubItemsAvailableMovements()
        {
            for (int x = 0; x < Items.Count; x++)
            {
                CollectionOrganizationItemViewModel colItemVm = Items[x];
                bool canMoveUp = x > 2; // Variables and Environments must remain at their positions
                bool canMoveDown = x < Items.Count - 1;
                colItemVm.CanMoveUp = canMoveUp;
                colItemVm.CanMoveDown = canMoveDown;
            }
        }
        
        private void AddNewFolder()
        {
            PororocaCollectionFolder newFolder = new(Localizer.Instance["Folder/NewFolder"]);
            AddFolder(newFolder);
        }

        private void AddNewRequest()
        {
            PororocaRequest newReq = new(Localizer.Instance["Request/NewRequest"]);
            AddRequest(newReq);
        }

        private void AddNewEnvironment()
        {
            EnvironmentsGroupViewModel environmentsGroup = (EnvironmentsGroupViewModel) Items.First(i => i is EnvironmentsGroupViewModel);
            environmentsGroup.AddNewEnvironment();
        }

        private Task ImportEnvironmentsAsync()
        {
            EnvironmentsGroupViewModel environmentsGroup = (EnvironmentsGroupViewModel) Items.First(i => i is EnvironmentsGroupViewModel);
            return environmentsGroup.ImportEnvironmentsAsync();
        }

        public void AddFolder(PororocaCollectionFolder folderToAdd)
        {
            CollectionOrganizationItemViewModel variablesGroup = Items.First(i => i is CollectionVariablesViewModel);
            CollectionOrganizationItemViewModel environmentsGroup = Items.First(i => i is EnvironmentsGroupViewModel);
            IEnumerable<CollectionOrganizationItemViewModel> existingFolders = Items.Where(i => i is CollectionFolderViewModel);
            IEnumerable<CollectionOrganizationItemViewModel> existingRequests = Items.Where(i => i is RequestViewModel);
            CollectionFolderViewModel folderToAddVm = new(this, this, folderToAdd);

            CollectionOrganizationItemViewModel[] rearrangedItems = new [] { variablesGroup, environmentsGroup }
                                                                    .Concat(existingFolders)
                                                                    .Append(folderToAddVm)
                                                                    .Concat(existingRequests)
                                                                    .ToArray();
            Items.Clear();
            foreach (CollectionOrganizationItemViewModel item in rearrangedItems)
            {
                Items.Add(item);
            }
            RefreshSubItemsAvailableMovements();
        }

        public void AddRequest(PororocaRequest reqToAdd)
        {
            CollectionOrganizationItemViewModel variablesGroup = Items.First(i => i is CollectionVariablesViewModel);
            CollectionOrganizationItemViewModel environmentsGroup = Items.First(i => i is EnvironmentsGroupViewModel);
            IEnumerable<CollectionOrganizationItemViewModel> existingFolders = Items.Where(i => i is CollectionFolderViewModel);
            IEnumerable<CollectionOrganizationItemViewModel> existingRequests = Items.Where(i => i is RequestViewModel);
            RequestViewModel reqToAddVm = new(this, this, reqToAdd);

            CollectionOrganizationItemViewModel[] rearrangedItems = new [] { variablesGroup, environmentsGroup }
                                                                    .Concat(existingFolders)
                                                                    .Concat(existingRequests)
                                                                    .Append(reqToAddVm)
                                                                    .ToArray();

            Items.Clear();
            foreach (CollectionOrganizationItemViewModel item in rearrangedItems)
            {
                Items.Add(item);
            }

            RefreshSubItemsAvailableMovements();
        }

        protected override void CopyThis() =>
            throw new NotImplementedException();
            
        public override void PasteToThis()
        {
            ICloneable? itemToPaste = ClipboardAreaDataCtx.FetchCopy();
            if (itemToPaste is PororocaCollectionFolder folderToPaste)
                AddFolder(folderToPaste);
            else if (itemToPaste is PororocaRequest reqToPaste)
                AddRequest(reqToPaste);
            else if (itemToPaste is PororocaEnvironment)
            {
                EnvironmentsGroupViewModel envGpVm = (EnvironmentsGroupViewModel)Items.First(i => i is EnvironmentsGroupViewModel);
                envGpVm.PasteToThis();
            }
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
                PororocaCollection c = ToCollection();
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
            PororocaCollection newCol = new(_colId, Name, _colCreatedAt);
            foreach (CollectionOrganizationItemViewModel colItemVm in Items)
            {
                if (colItemVm is CollectionVariablesViewModel colVarsVm)
                    newCol.UpdateVariables(colVarsVm.ToVariables());
                else if (colItemVm is EnvironmentsGroupViewModel colEnvsVm)
                    newCol.UpdateEnvironments(colEnvsVm.ToEnvironments());
                else if (colItemVm is CollectionFolderViewModel colFolderVm)
                    newCol.AddFolder(colFolderVm.ToCollectionFolder());
                else if (colItemVm is RequestViewModel reqVm)
                    newCol.AddRequest(reqVm.ToRequest());
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
                IEnumerable<PororocaVariable> collectionVariables = ((CollectionVariablesViewModel)Items.First(i => i is CollectionVariablesViewModel)).ToVariables();
                IEnumerable<PororocaVariable>? environmentVariables = ((EnvironmentsGroupViewModel)Items.First(i => i is EnvironmentsGroupViewModel))
                                                                      .Items
                                                                      .FirstOrDefault(evm => evm.IsCurrentEnvironment)
                                                                      ?.ToEnvironment()
                                                                      ?.Variables;
                IEnumerable<PororocaVariable> effectiveVariables = PororocaVariablesMerger.MergeVariables(collectionVariables, environmentVariables);
                string resolvedStr = strToReplaceTemplatedVariables!;
                foreach (PororocaVariable v in effectiveVariables)
                {
                    string variableTemplate = VariableTemplateBeginToken + v.Key + VariableTemplateEndToken;
                    resolvedStr = resolvedStr.Replace(variableTemplate, v.Value ?? string.Empty);
                }
                return resolvedStr;
            }
        }

        #endregion
    }
}