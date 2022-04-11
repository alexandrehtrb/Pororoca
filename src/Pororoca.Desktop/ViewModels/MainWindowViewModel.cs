using System.Reactive;
using System.Text;
using Avalonia.Controls;
using ReactiveUI;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Entities.Pororoca;
using static Pororoca.Domain.Features.ImportCollection.PororocaCollectionImporter;
using static Pororoca.Domain.Features.ImportCollection.PostmanCollectionV21Importer;
using static Pororoca.Domain.Features.Common.AvailablePororocaRequestSelectionOptions;
using Pororoca.Desktop.Localization;
using Pororoca.Desktop.UserData;

namespace Pororoca.Desktop.ViewModels
{
    public sealed class MainWindowViewModel : ViewModelBase, ICollectionOrganizationItemParentViewModel
    {
        #region COLLECTIONS ORGANIZATION

        private CollectionsGroupViewModel _collectionsGroupViewDataCtx;
        public CollectionsGroupViewModel CollectionsGroupViewDataCtx
        {
            get => _collectionsGroupViewDataCtx;
            set
            {
                this.RaiseAndSetIfChanged(ref _collectionsGroupViewDataCtx, value);
            }
        }

        public Action<CollectionOrganizationItemViewModel> OnRenameSubItemSelected => onRenameItemSelected;

        Action ICollectionOrganizationItemParentViewModel.OnAfterItemDeleted => onAfterItemDeleted;

        public ReactiveCommand<Unit, Unit> AddNewCollectionCmd { get; }
        public ReactiveCommand<Unit, Unit> ImportCollectionsCmd { get; }

        #endregion

        #region SCREENS

        private bool _isCollectionViewVisible = false;
        public bool IsCollectionViewVisible
        {
            get => _isCollectionViewVisible;
            set
            {
                this.RaiseAndSetIfChanged(ref _isCollectionViewVisible, value);
            }
        }
        private CollectionViewModel? _collectionViewDataCtx = null;
        public CollectionViewModel? CollectionViewDataCtx
        {
            get => _collectionViewDataCtx;
            set
            {
                this.RaiseAndSetIfChanged(ref _collectionViewDataCtx, value);
            }
        }

        private bool _isCollectionVariablesViewVisible = false;
        public bool IsCollectionVariablesViewVisible
        {
            get => _isCollectionVariablesViewVisible;
            set
            {
                this.RaiseAndSetIfChanged(ref _isCollectionVariablesViewVisible, value);
            }
        }
        private CollectionVariablesViewModel? _collectionVariablesViewDataCtx = null;
        public CollectionVariablesViewModel? CollectionVariablesViewDataCtx
        {
            get => _collectionVariablesViewDataCtx;
            set
            {
                this.RaiseAndSetIfChanged(ref _collectionVariablesViewDataCtx, value);
            }
        }

        private bool _isEnvironmentViewVisible = false;
        public bool IsEnvironmentViewVisible
        {
            get => _isEnvironmentViewVisible;
            set
            {
                this.RaiseAndSetIfChanged(ref _isEnvironmentViewVisible, value);
            }
        }
        private EnvironmentViewModel? _environmentViewDataCtx = null;
        public EnvironmentViewModel? EnvironmentViewDataCtx
        {
            get => _environmentViewDataCtx;
            set
            {
                this.RaiseAndSetIfChanged(ref _environmentViewDataCtx, value);
            }
        }

        private bool _isCollectionFolderViewVisible = false;
        public bool IsCollectionFolderViewVisible
        {
            get => _isCollectionFolderViewVisible;
            set
            {
                this.RaiseAndSetIfChanged(ref _isCollectionFolderViewVisible, value);
            }
        }
        private CollectionFolderViewModel? _collectionFolderViewDataCtx = null;
        public CollectionFolderViewModel? CollectionFolderViewDataCtx
        {
            get => _collectionFolderViewDataCtx;
            set
            {
                this.RaiseAndSetIfChanged(ref _collectionFolderViewDataCtx, value);
            }
        }

        private bool _isRequestViewVisible = false;
        public bool IsRequestViewVisible
        {
            get => _isRequestViewVisible;
            set
            {
                this.RaiseAndSetIfChanged(ref _isRequestViewVisible, value);
            }
        }
        private RequestViewModel? _requestViewDataCtx = null;
        public RequestViewModel? RequestViewDataCtx
        {
            get => _requestViewDataCtx;
            set
            {
                this.RaiseAndSetIfChanged(ref _requestViewDataCtx, value);
            }
        }

        #endregion

        #region LANGUAGE

        private bool _isLanguagePortuguese = false;
        public bool IsLanguagePortuguese
        {
            get => _isLanguagePortuguese;
            set
            {
                this.RaiseAndSetIfChanged(ref _isLanguagePortuguese, value);
            }
        }

        private bool _isLanguageEnglish = false;
        public bool IsLanguageEnglish
        {
            get => _isLanguageEnglish;
            set
            {
                this.RaiseAndSetIfChanged(ref _isLanguageEnglish, value);
            }
        }

        public ReactiveCommand<Unit, Unit> SelectLanguagePortuguesCmd { get; }
        public ReactiveCommand<Unit, Unit> SelectLanguageEnglishCmd { get; }

        #endregion

        #region GLOBAL OPTIONS

        private bool _isSslVerificationDisabled = false;
        public bool IsSslVerificationDisabled
        {
            get => _isSslVerificationDisabled;
            set
            {
                this.RaiseAndSetIfChanged(ref _isSslVerificationDisabled, value);
            }
        }

        public ReactiveCommand<Unit, Unit> ToggleSSLVerificationCmd { get; }

        #endregion

        #region OTHERS

        private readonly bool isOperatingSystemMacOsx;

        #endregion

        public MainWindowViewModel(Func<bool>? isOperatingSystemMacOsx = null)
        {
            #region OTHERS
            this.isOperatingSystemMacOsx = (isOperatingSystemMacOsx ?? OperatingSystem.IsMacOS)();
            #endregion

            #region COLLECTIONS ORGANIZATION
            _collectionsGroupViewDataCtx = new(this, OnCollectionsGroupItemSelected);
            ImportCollectionsCmd = ReactiveCommand.CreateFromTask(ImportCollectionsAsync);
            AddNewCollectionCmd = ReactiveCommand.Create(AddNewCollection);
            #endregion

            #region LANGUAGE
            SelectLanguagePortuguesCmd = ReactiveCommand.Create(() => SelectLanguage(Language.PtBr));
            SelectLanguageEnglishCmd = ReactiveCommand.Create(() => SelectLanguage(Language.EnGb));
            #endregion

            #region GLOBAL OPTIONS
            ToggleSSLVerificationCmd = ReactiveCommand.Create(ToggleSslVerification);
            #endregion

            #region USER DATA
            LoadUserData();
            #endregion
        }

        #region SCREENS

        private void OnCollectionsGroupItemSelected(ViewModelBase? selectedItem)
        {
            if (selectedItem is CollectionViewModel colVm)
            {
                CollectionViewDataCtx = colVm;
                if (!IsCollectionViewVisible)
                {
                    IsCollectionViewVisible = true;
                    IsCollectionVariablesViewVisible = false;
                    IsEnvironmentViewVisible = false;
                    IsCollectionFolderViewVisible = false;
                    IsRequestViewVisible = false;
                }
            }
            else if (selectedItem is CollectionVariablesViewModel colVarsVm)
            {
                CollectionVariablesViewDataCtx = colVarsVm;
                if (!IsCollectionVariablesViewVisible)
                {
                    IsCollectionViewVisible = false;
                    IsCollectionVariablesViewVisible = true;
                    IsEnvironmentViewVisible = false;
                    IsCollectionFolderViewVisible = false;
                    IsRequestViewVisible = false;
                }
            }
            else if (selectedItem is EnvironmentViewModel envVm)
            {
                EnvironmentViewDataCtx = envVm;
                if (!IsEnvironmentViewVisible)
                {
                    IsCollectionViewVisible = false;
                    IsCollectionVariablesViewVisible = false;
                    IsEnvironmentViewVisible = true;
                    IsCollectionFolderViewVisible = false;
                    IsRequestViewVisible = false;
                }
            }
            else if (selectedItem is CollectionFolderViewModel folderVm)
            {
                CollectionFolderViewDataCtx = folderVm;
                if (!IsCollectionFolderViewVisible)
                {
                    IsCollectionViewVisible = false;
                    IsCollectionVariablesViewVisible = false;
                    IsEnvironmentViewVisible = false;
                    IsCollectionFolderViewVisible = true;
                    IsRequestViewVisible = false;
                }
            }
            else if (selectedItem is RequestViewModel reqVm)
            {
                RequestViewDataCtx = reqVm;
                if (!IsRequestViewVisible)
                {
                    IsCollectionViewVisible = false;
                    IsCollectionVariablesViewVisible = false;
                    IsEnvironmentViewVisible = false;
                    IsCollectionFolderViewVisible = false;
                    IsRequestViewVisible = true;
                }
            }
        }

        #endregion

        #region COLLECTIONS ORGANIZATION

        public void MoveSubItem(ICollectionOrganizationItemViewModel colItemVm, MoveableItemMovementDirection direction) =>
            CollectionsGroupViewDataCtx.MoveSubItem(colItemVm, direction);

        private void AddNewCollection()
        {
            PororocaCollection newCol = new(Localizer.Instance["Collection/NewCollection"]);
            AddCollection(newCol);
        }

        private void AddCollection(PororocaCollection col)
        {
            CollectionViewModel colVm = new(this, col, DuplicateCollection);
            CollectionsGroupViewDataCtx.Items.Add(colVm);
            CollectionsGroupViewDataCtx.RefreshSubItemsAvailableMovements();
        }

        private void DuplicateCollection(CollectionViewModel colVm)
        {
            PororocaCollection collectionCopy = (PororocaCollection)colVm.ToCollection().Clone();
            AddCollection(collectionCopy);
        }

        private void onRenameItemSelected(ViewModelBase vm) =>
            OnCollectionsGroupItemSelected(vm);

        public void DeleteSubItem(ICollectionOrganizationItemViewModel colVm)
        {
            CollectionsGroupViewDataCtx.Items.Remove((CollectionViewModel)colVm);
            CollectionsGroupViewDataCtx.RefreshSubItemsAvailableMovements();
            onAfterItemDeleted();
        }

        private void onAfterItemDeleted()
        {
            IsCollectionViewVisible = false;
            IsCollectionVariablesViewVisible = false;
            IsEnvironmentViewVisible = false;
            IsCollectionFolderViewVisible = false;
            IsRequestViewVisible = false;
        }

        private async Task ImportCollectionsAsync()
        {
            List<FileDialogFilter> fileSelectionfilters = new();
            // Mac OSX file dialogs have problems with file filters... TODO: find if there is a way to solve this
            if (!isOperatingSystemMacOsx)
            {
                fileSelectionfilters.Add(
                    new()
                    {
                        Name = Localizer.Instance["Collection/ImportCollectionDialogTypes"],
                        Extensions = new List<string> { PororocaCollectionExtension, PostmanCollectionExtension }
                    }
                );
            }

            OpenFileDialog dialog = new()
            {
                Title = Localizer.Instance["Collection/ImportCollectionDialogTitle"],
                AllowMultiple = true,
                Filters = fileSelectionfilters
            };
            string[]? result = await dialog.ShowAsync(MainWindow.Instance!);
            if (result != null)
            {
                foreach (string collectionFilePath in result)
                {
                    // First, tries to import as a Pororoca collection
                    if (collectionFilePath.EndsWith(PororocaCollectionExtension))
                    {
                        string fileContent = await File.ReadAllTextAsync(collectionFilePath, Encoding.UTF8);
                        if (TryImportPororocaCollection(fileContent, out PororocaCollection? importedPororocaCollection))
                        {
                            AddCollection(importedPororocaCollection!);
                        }
                    }
                    // If not a valid Pororoca collection, then tries to import as a Postman collection
                    else if (collectionFilePath.EndsWith(PostmanCollectionExtension))
                    {
                        string fileContent = await File.ReadAllTextAsync(collectionFilePath, Encoding.UTF8);
                        if (TryImportPostmanCollection(fileContent, out PororocaCollection? importedPostmanCollection))
                        {
                            AddCollection(importedPostmanCollection!);
                        }
                    }
                }
            }
        }

        #endregion

        #region LANGUAGE

        private void SelectLanguage(Language lang)
        {
            Localizer.Instance.LoadLanguage(lang);
            switch (lang)
            {
                case Language.PtBr:
                    IsLanguagePortuguese = true;
                    IsLanguageEnglish = false;
                    break;
                default:
                case Language.EnGb:
                    IsLanguagePortuguese = false;
                    IsLanguageEnglish = true;
                    break;
            }
        }

        #endregion

        #region GLOBAL OPTIONS

        private void ToggleSslVerification() =>
            IsSslVerificationDisabled = !IsSslVerificationDisabled;

        #endregion

        #region USER DATA

        private void LoadUserData()
        {
            UserPreferences? userPrefs = UserDataManager.LoadUserPreferences();
            PororocaCollection[] cols = UserDataManager.LoadUserCollections();

            if (userPrefs != null)
            {
                Language lang = LanguageExtensions.GetLanguageFromLCID(userPrefs.Lang);
                SelectLanguage(lang);
            }
            else
            {
                SelectLanguage(Language.EnGb);
            }
            foreach (PororocaCollection col in cols)
            {
                AddCollection(col);
            }
        }

        public void SaveUserData()
        {
            UserPreferences userPrefs = new() { Lang = Localizer.Instance.Language.GetLanguageLCID() };
            PororocaCollection[] cols = CollectionsGroupViewDataCtx
                                        .Items
                                        .Select(cvm => cvm.ToCollection())
                                        .DistinctBy(c => c.Id)
                                        .ToArray();

            UserDataManager.SaveUserData(userPrefs, cols).GetAwaiter().GetResult();
        }

        #endregion
    }
}