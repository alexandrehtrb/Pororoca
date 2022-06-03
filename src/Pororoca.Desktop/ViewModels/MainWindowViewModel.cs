using System.Reactive;
using System.Text;
using Avalonia.Controls;
using Pororoca.Desktop.Localization;
using Pororoca.Desktop.UserData;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Entities.Pororoca;
using ReactiveUI;
using static Pororoca.Domain.Features.Common.AvailablePororocaRequestSelectionOptions;
using static Pororoca.Domain.Features.ImportCollection.PororocaCollectionImporter;
using static Pororoca.Domain.Features.ImportCollection.PostmanCollectionV21Importer;

namespace Pororoca.Desktop.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase, ICollectionOrganizationItemParentViewModel
{
    #region COLLECTIONS ORGANIZATION

    private CollectionsGroupViewModel collectionsGroupViewDataCtxField;
    public CollectionsGroupViewModel CollectionsGroupViewDataCtx
    {
        get => this.collectionsGroupViewDataCtxField;
        set => this.RaiseAndSetIfChanged(ref this.collectionsGroupViewDataCtxField, value);
    }

    public Action<CollectionOrganizationItemViewModel> OnRenameSubItemSelected => onRenameItemSelected;

    Action ICollectionOrganizationItemParentViewModel.OnAfterItemDeleted => onAfterItemDeleted;

    public ReactiveCommand<Unit, Unit> AddNewCollectionCmd { get; }
    public ReactiveCommand<Unit, Unit> ImportCollectionsCmd { get; }

    #endregion

    #region SCREENS

    private bool isCollectionViewVisibleField = false;
    public bool IsCollectionViewVisible
    {
        get => this.isCollectionViewVisibleField;
        set => this.RaiseAndSetIfChanged(ref this.isCollectionViewVisibleField, value);
    }
    private CollectionViewModel? collectionViewDataCtxField = null;
    public CollectionViewModel? CollectionViewDataCtx
    {
        get => this.collectionViewDataCtxField;
        set => this.RaiseAndSetIfChanged(ref this.collectionViewDataCtxField, value);
    }

    private bool isCollectionVariablesViewVisibleField = false;
    public bool IsCollectionVariablesViewVisible
    {
        get => this.isCollectionVariablesViewVisibleField;
        set => this.RaiseAndSetIfChanged(ref this.isCollectionVariablesViewVisibleField, value);
    }
    private CollectionVariablesViewModel? collectionVariablesViewDataCtxField = null;
    public CollectionVariablesViewModel? CollectionVariablesViewDataCtx
    {
        get => this.collectionVariablesViewDataCtxField;
        set => this.RaiseAndSetIfChanged(ref this.collectionVariablesViewDataCtxField, value);
    }

    private bool isEnvironmentViewVisibleField = false;
    public bool IsEnvironmentViewVisible
    {
        get => this.isEnvironmentViewVisibleField;
        set => this.RaiseAndSetIfChanged(ref this.isEnvironmentViewVisibleField, value);
    }
    private EnvironmentViewModel? environmentViewDataCtxField = null;
    public EnvironmentViewModel? EnvironmentViewDataCtx
    {
        get => this.environmentViewDataCtxField;
        set => this.RaiseAndSetIfChanged(ref this.environmentViewDataCtxField, value);
    }

    private bool isCollectionFolderViewVisibleField = false;
    public bool IsCollectionFolderViewVisible
    {
        get => this.isCollectionFolderViewVisibleField;
        set => this.RaiseAndSetIfChanged(ref this.isCollectionFolderViewVisibleField, value);
    }
    private CollectionFolderViewModel? collectionFolderViewDataCtxField = null;
    public CollectionFolderViewModel? CollectionFolderViewDataCtx
    {
        get => this.collectionFolderViewDataCtxField;
        set => this.RaiseAndSetIfChanged(ref this.collectionFolderViewDataCtxField, value);
    }

    private bool isRequestViewVisibleField = false;
    public bool IsRequestViewVisible
    {
        get => this.isRequestViewVisibleField;
        set => this.RaiseAndSetIfChanged(ref this.isRequestViewVisibleField, value);
    }
    private RequestViewModel? requestViewDataCtxField = null;
    public RequestViewModel? RequestViewDataCtx
    {
        get => this.requestViewDataCtxField;
        set => this.RaiseAndSetIfChanged(ref this.requestViewDataCtxField, value);
    }

    #endregion

    #region LANGUAGE

    private bool isLanguagePortugueseField = false;
    public bool IsLanguagePortuguese
    {
        get => this.isLanguagePortugueseField;
        set => this.RaiseAndSetIfChanged(ref this.isLanguagePortugueseField, value);
    }

    private bool isLanguageEnglishField = false;
    public bool IsLanguageEnglish
    {
        get => this.isLanguageEnglishField;
        set => this.RaiseAndSetIfChanged(ref this.isLanguageEnglishField, value);
    }

    public ReactiveCommand<Unit, Unit> SelectLanguagePortuguesCmd { get; }
    public ReactiveCommand<Unit, Unit> SelectLanguageEnglishCmd { get; }

    #endregion

    #region GLOBAL OPTIONS

    private bool isSslVerificationDisabledField = false;
    public bool IsSslVerificationDisabled
    {
        get => this.isSslVerificationDisabledField;
        set => this.RaiseAndSetIfChanged(ref this.isSslVerificationDisabledField, value);
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
        this.collectionsGroupViewDataCtxField = new(this, OnCollectionsGroupItemSelected);
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
        var collectionCopy = (PororocaCollection)colVm.ToCollection().Clone();
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
        if (!this.isOperatingSystemMacOsx)
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
                    if (TryImportPororocaCollection(fileContent, out var importedPororocaCollection))
                    {
                        AddCollection(importedPororocaCollection!);
                    }
                }
                // If not a valid Pororoca collection, then tries to import as a Postman collection
                else if (collectionFilePath.EndsWith(PostmanCollectionExtension))
                {
                    string fileContent = await File.ReadAllTextAsync(collectionFilePath, Encoding.UTF8);
                    if (TryImportPostmanCollection(fileContent, out var importedPostmanCollection))
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
        var userPrefs = UserDataManager.LoadUserPreferences();
        var cols = UserDataManager.LoadUserCollections();

        if (userPrefs != null)
        {
            var lang = LanguageExtensions.GetLanguageFromLCID(userPrefs.Lang);
            SelectLanguage(lang);
        }
        else
        {
            SelectLanguage(Language.EnGb);
        }
        foreach (var col in cols)
        {
            AddCollection(col);
        }
    }

    public void SaveUserData()
    {
        UserPreferences userPrefs = new() { Lang = Localizer.Instance.Language.GetLanguageLCID() };
        var cols = CollectionsGroupViewDataCtx
                                    .Items
                                    .Select(cvm => cvm.ToCollection())
                                    .DistinctBy(c => c.Id)
                                    .ToArray();

        UserDataManager.SaveUserData(userPrefs, cols).GetAwaiter().GetResult();
    }

    #endregion
}