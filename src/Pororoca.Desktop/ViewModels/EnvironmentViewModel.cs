using System.Collections.ObjectModel;
using System.Reactive;
using System.Text;
using Avalonia.Controls;
using ReactiveUI;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.ExportEnvironment;
using Pororoca.Desktop.Localization;
using static Pororoca.Domain.Features.Common.AvailablePororocaRequestSelectionOptions;

namespace Pororoca.Desktop.ViewModels
{
    public sealed class EnvironmentViewModel : CollectionOrganizationItemViewModel
    {
        #region COLLECTION ORGANIZATION

        public ReactiveCommand<Unit, Unit> MoveUpCmd { get; }
        public ReactiveCommand<Unit, Unit> MoveDownCmd { get; }
        public ReactiveCommand<Unit, Unit> CopyEnvironmentCmd { get; }
        public ReactiveCommand<Unit, Unit> RenameEnvironmentCmd { get; }
        public ReactiveCommand<Unit, Unit> DeleteEnvironmentCmd { get; }
        public ReactiveCommand<Unit, Unit> ExportEnvironmentCmd { get; }
        public ReactiveCommand<Unit, Unit> ExportAsPororocaEnvironmentCmd { get; }
        public ReactiveCommand<Unit, Unit> ExportAsPostmanEnvironmentCmd { get; }

        #endregion

        #region ENVIRONMENT

        private readonly Guid _envId;
        private readonly DateTimeOffset _envCreatedAt;
        private readonly Action<EnvironmentViewModel> _onEnvironmentSetAsCurrent;

        private bool _isCurrentEnvironment;
        public bool IsCurrentEnvironment
        {
            get => _isCurrentEnvironment;
            set
            {
                this.RaiseAndSetIfChanged(ref _isCurrentEnvironment, value);
            }
        }

        private bool _includeSecretVariables;
        public bool IncludeSecretVariables
        {
            get => _includeSecretVariables;
            set
            {
                this.RaiseAndSetIfChanged(ref _includeSecretVariables, value);
            }
        }

        public ObservableCollection<VariableViewModel> Variables { get; }
        public VariableViewModel? SelectedVariable { get; set; }
        public ReactiveCommand<Unit, Unit> AddNewVariableCmd { get; }
        public ReactiveCommand<Unit, Unit> RemoveSelectedVariableCmd { get; }
        public ReactiveCommand<Unit, Unit> SetAsCurrentEnvironmentCmd { get; }

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

        public EnvironmentViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                                    PororocaEnvironment env,
                                    Action<EnvironmentViewModel> onEnvironmentSetAsCurrent,
                                    Func<bool>? isOperatingSystemMacOsx = null) : base(parentVm, env.Name)
        {
            #region OTHERS
            _isOperatingSystemMacOsx = (isOperatingSystemMacOsx ?? OperatingSystem.IsMacOS)();
            #endregion

            #region COLLECTION ORGANIZATION
            
            MoveUpCmd = ReactiveCommand.Create(MoveThisUp);
            MoveDownCmd = ReactiveCommand.Create(MoveThisDown);
            CopyEnvironmentCmd = ReactiveCommand.Create(Copy);
            RenameEnvironmentCmd = ReactiveCommand.Create(RenameThis);
            DeleteEnvironmentCmd = ReactiveCommand.Create(Delete);
            ExportEnvironmentCmd = ReactiveCommand.CreateFromTask(ExportEnvironmentAsync);
            ExportAsPororocaEnvironmentCmd = ReactiveCommand.CreateFromTask(ExportAsPororocaEnvironmentAsync);
            ExportAsPostmanEnvironmentCmd = ReactiveCommand.CreateFromTask(ExportAsPostmanEnvironmentAsync);

            #endregion

            #region ENVIRONMENT
            
            _envId = env.Id;
            _envCreatedAt = env.CreatedAt;
            _onEnvironmentSetAsCurrent = onEnvironmentSetAsCurrent;

            Variables = new(env.Variables.Select(v => new VariableViewModel(v)));
            IsCurrentEnvironment = env.IsCurrent;
            AddNewVariableCmd = ReactiveCommand.Create(AddNewVariable);
            RemoveSelectedVariableCmd = ReactiveCommand.Create(RemoveSelectedVariable);
            SetAsCurrentEnvironmentCmd = ReactiveCommand.Create(SetAsCurrentEnvironment);

            #endregion
        }

        #region COLLECTION ORGANIZATION

        protected override void CopyThis() =>
            CollectionsGroupDataCtx.PushToCopy(ToEnvironment());

        #region EXPORT ENVIRONMENT

        private Task ExportEnvironmentAsync()
        {
            SaveFileDialog saveFileDialog = new()
            {
                Title = Localizer.Instance["Environment/ExportEnvironmentDialogTitle"],
                Filters = new()
                {
                    new()
                    {
                        Name = Localizer.Instance["Environment/PororocaEnvironmentFormat"],
                        Extensions = new List<string> { PororocaEnvironmentExtension }
                    },
                    new()
                    {
                        Name = Localizer.Instance["Environment/PostmanEnvironmentFormat"],
                        Extensions = new List<string> { PostmanEnvironmentExtension }
                    }
                }
            };

            return ShowExportEnvironmentDialogAsync(saveFileDialog);
        }

        private Task ExportAsPororocaEnvironmentAsync()
        {
            SaveFileDialog saveFileDialog = new()
            {
                Title = Localizer.Instance["Environment/ExportAsPororocaEnvironmentDialogTitle"],
                InitialFileName = $"{Name}.{PororocaEnvironmentExtension}"
            };
            
            return ShowExportEnvironmentDialogAsync(saveFileDialog);
        }

        private Task ExportAsPostmanEnvironmentAsync()
        {
            SaveFileDialog saveFileDialog = new()
            {
                Title = Localizer.Instance["Environment/ExportAsPostmanEnvironmentDialogTitle"],
                InitialFileName = $"{Name}.{PostmanEnvironmentExtension}"
            };
            
            return ShowExportEnvironmentDialogAsync(saveFileDialog);
        }

        private async Task ShowExportEnvironmentDialogAsync(SaveFileDialog saveFileDialog)
        {
            string? saveFileOutputPath = await saveFileDialog.ShowAsync(MainWindow.Instance!);
            if (saveFileOutputPath != null)
            {
                PororocaEnvironment env = ToEnvironment();
                string json = saveFileOutputPath.EndsWith(PostmanEnvironmentExtension) ?
                    PostmanEnvironmentExporter.ExportAsPostmanEnvironment(env, !IncludeSecretVariables) :
                    PororocaEnvironmentExporter.ExportAsPororocaEnvironment(env, !IncludeSecretVariables);
                await File.WriteAllTextAsync(saveFileOutputPath, json, Encoding.UTF8);
            }
        }

        #endregion

        #endregion

        #region ENVIRONMENT

        private void SetAsCurrentEnvironment() =>
            _onEnvironmentSetAsCurrent(this);

        private void AddNewVariable() =>
            Variables.Add(new(true, string.Empty, string.Empty, false));

        private void RemoveSelectedVariable()
        {
            if (SelectedVariable != null)
            {
                Variables.Remove(SelectedVariable);
                SelectedVariable = null;
            }
            else if (Variables.Count == 1)
            {
                Variables.Clear();
            }
        }

        public PororocaEnvironment ToEnvironment()
        {
            PororocaEnvironment newEnv = new(_envId, Name, _envCreatedAt);
            UpdateEnvironmentWithInputs(newEnv);
            return newEnv;
        }

        private void UpdateEnvironmentWithInputs(PororocaEnvironment environment)
        {
            environment.IsCurrent = IsCurrentEnvironment;
            environment.UpdateVariables(Variables.Select(v => v.ToVariable()));
        }

        #endregion
    }
}