using System.Text;
using Avalonia.Platform.Storage;
using Pororoca.Desktop.Localization;
using Pororoca.Desktop.Views;
using Pororoca.Desktop.ViewModels;
using static Pororoca.Domain.Features.ExportCollection.PostmanCollectionV21Exporter;
using static Pororoca.Domain.Features.ExportCollection.PororocaCollectionExporter;
using static Pororoca.Domain.Features.ExportEnvironment.PororocaEnvironmentExporter;
using static Pororoca.Domain.Features.ExportEnvironment.PostmanEnvironmentExporter;
using static Pororoca.Domain.Features.ImportCollection.PororocaCollectionImporter;
using static Pororoca.Domain.Features.ImportCollection.PostmanCollectionV21Importer;
using static Pororoca.Domain.Features.ImportEnvironment.PororocaEnvironmentImporter;
using static Pororoca.Domain.Features.ImportEnvironment.PostmanEnvironmentImporter;

namespace Pororoca.Desktop.ExportImport;

internal static class FileExporterImporter
{
    internal const string PororocaCollectionExtension = "pororoca_collection.json";
    internal const string PostmanCollectionExtension = "postman_collection.json";
    internal const string PororocaEnvironmentExtension = "pororoca_environment.json";
    internal const string PostmanEnvironmentExtension = "postman_environment.json";

    private const string PororocaCollectionExtensionGlob = $"*.{PororocaCollectionExtension}";
    private const string PostmanCollectionExtensionGlob = $"*.{PostmanCollectionExtension}";
    private const string PororocaEnvironmentExtensionGlob = $"*.{PororocaEnvironmentExtension}";
    private const string PostmanEnvironmentExtensionGlob = $"*.{PostmanEnvironmentExtension}";


    #region EXPORT COLLECTION

    internal static Task ExportCollectionAsync(CollectionViewModel cvm)
    {
        FilePickerSaveOptions opts = new()
        {
            Title = Localizer.Instance["Collection/ExportCollectionDialogTitle"],
            SuggestedFileName = cvm.Name,
            FileTypeChoices = new List<FilePickerFileType>()
            {
                new(Localizer.Instance["Collection/PororocaCollectionFormat"])
                {
                    Patterns = new List<string> { PororocaCollectionExtensionGlob }
                },
                new(Localizer.Instance["Collection/PostmanCollectionFormat"])
                {
                    Patterns = new List<string> { PostmanCollectionExtensionGlob }
                }
            }
        };

        return ShowExportCollectionDialogAsync(cvm, opts);
    }

    internal static Task ExportAsPororocaCollectionAsync(CollectionViewModel cvm)
    {
        FilePickerSaveOptions opts = new()
        {
            Title = Localizer.Instance["Collection/ExportAsPororocaCollectionDialogTitle"],
            SuggestedFileName = $"{cvm.Name}.{PororocaCollectionExtension}"
        };

        return ShowExportCollectionDialogAsync(cvm, opts);
    }

    internal static Task ExportAsPostmanCollectionAsync(CollectionViewModel cvm)
    {
        FilePickerSaveOptions opts = new()
        {
            Title = Localizer.Instance["Collection/ExportAsPostmanCollectionDialogTitle"],
            SuggestedFileName = $"{cvm.Name}.{PostmanCollectionExtension}"
        };

        return ShowExportCollectionDialogAsync(cvm, opts);
    }

    private static async Task ShowExportCollectionDialogAsync(CollectionViewModel cvm, FilePickerSaveOptions opts)
    {
        var destFilePath = await SelectPathForFileToBeSavedAsync(opts);

        if (destFilePath != null)
        {
            var c = cvm.ToCollection();
            string json = destFilePath.EndsWith(PostmanCollectionExtension) ?
                ExportAsPostmanCollectionV21(c, !cvm.IncludeSecretVariables) :
                ExportAsPororocaCollection(c, !cvm.IncludeSecretVariables);
            await File.WriteAllTextAsync(destFilePath, json, Encoding.UTF8);
        }
    }

    #endregion

    #region EXPORT ENVIRONMENT

    internal static Task ExportEnvironmentAsync(EnvironmentViewModel evm)
    {
        FilePickerSaveOptions opts = new()
        {
            Title = Localizer.Instance["Environment/ExportEnvironmentDialogTitle"],
            SuggestedFileName = evm.Name,
            FileTypeChoices = new List<FilePickerFileType>()
            {
                new(Localizer.Instance["Environment/PororocaEnvironmentFormat"])
                {
                    Patterns = new List<string> { PororocaEnvironmentExtensionGlob }
                },
                new(Localizer.Instance["Environment/PostmanEnvironmentFormat"])
                {
                    Patterns = new List<string> { PostmanEnvironmentExtensionGlob }
                }
            }
        };

        return ShowExportEnvironmentDialogAsync(evm, opts);
    }

    internal static Task ExportAsPororocaEnvironmentAsync(EnvironmentViewModel evm)
    {
        FilePickerSaveOptions opts = new()
        {
            Title = Localizer.Instance["Environment/ExportAsPororocaEnvironmentDialogTitle"],
            SuggestedFileName = $"{evm.Name}.{PororocaEnvironmentExtension}"
        };

        return ShowExportEnvironmentDialogAsync(evm, opts);
    }

    internal static Task ExportAsPostmanEnvironmentAsync(EnvironmentViewModel evm)
    {
        FilePickerSaveOptions opts = new()
        {
            Title = Localizer.Instance["Environment/ExportAsPostmanEnvironmentDialogTitle"],
            SuggestedFileName = $"{evm.Name}.{PostmanEnvironmentExtension}"
        };

        return ShowExportEnvironmentDialogAsync(evm, opts);
    }

    private static async Task ShowExportEnvironmentDialogAsync(EnvironmentViewModel evm, FilePickerSaveOptions opts)
    {
        var destFilePath = await SelectPathForFileToBeSavedAsync(opts);

        if (destFilePath != null)
        {
            var env = evm.ToEnvironment();
            string json = destFilePath.EndsWith(PostmanEnvironmentExtension) ?
                ExportAsPostmanEnvironment(env, !evm.IncludeSecretVariables) :
                ExportAsPororocaEnvironment(env, !evm.IncludeSecretVariables);
            await File.WriteAllTextAsync(destFilePath, json, Encoding.UTF8);
        }
    }

    #endregion

    #region IMPORT ENVIRONMENTS

    public static async Task ImportEnvironmentsAsync(EnvironmentsGroupViewModel egvm)
    {
        List<FilePickerFileType> fileSelectionfilters = new();
        // Mac OSX file dialogs have problems with file filters... TODO: find if there is a way to solve this
        if (!OperatingSystem.IsMacOS())
        {
            fileSelectionfilters.Add(
                new(Localizer.Instance["Collection/ImportEnvironmentDialogTypes"])
                {
                    Patterns = new List<string> { PororocaEnvironmentExtensionGlob, PostmanEnvironmentExtensionGlob }
                }
            );
        }

        FilePickerOpenOptions opts = new()
        {
            Title = Localizer.Instance["Collection/ImportEnvironmentDialogTitle"],
            AllowMultiple = true,
            FileTypeFilter = fileSelectionfilters
        };

        var filesPaths = await SelectFilesFromStorageAsync(opts);
        if (filesPaths != null)
        {
            foreach (var filePath in filesPaths)
            {
                // First, tries to import as a Pororoca environment
                if (filePath.EndsWith(PororocaEnvironmentExtension))
                {
                    string fileContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
                    if (TryImportPororocaEnvironment(fileContent, out var importedPororocaEnvironment))
                    {
                        importedPororocaEnvironment!.IsCurrent = false; // Imported environment should always be disabled
                        egvm.AddEnvironment(importedPororocaEnvironment!);
                    }
                }
                // If not a valid Pororoca collection, then tries to import as a Postman collection
                else if (filePath.EndsWith(PostmanEnvironmentExtension))
                {
                    string fileContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
                    if (TryImportPostmanEnvironment(fileContent, out var importedPostmanEnvironment))
                    {
                        egvm.AddEnvironment(importedPostmanEnvironment!);
                    }
                }
            }
        }
    }

    #endregion

    #region IMPORT COLLECTIONS

    internal static async Task ImportCollectionsAsync(MainWindowViewModel mwvm)
    {
        List<FilePickerFileType> fileSelectionfilters = new();
        // Mac OSX file dialogs have problems with file filters... TODO: find if there is a way to solve this
        if (!OperatingSystem.IsMacOS())
        {
            fileSelectionfilters.Add(
                new(Localizer.Instance["Collection/ImportCollectionDialogTypes"])
                {
                    Patterns = new List<string> { PororocaCollectionExtensionGlob, PostmanCollectionExtensionGlob }
                }
            );
        }

        FilePickerOpenOptions opts = new()
        {
            Title = Localizer.Instance["Collection/ImportCollectionDialogTitle"],
            AllowMultiple = true,
            FileTypeFilter = fileSelectionfilters
        };
        var filesPaths = await SelectFilesFromStorageAsync(opts);
        if (filesPaths != null)
        {
            foreach (var filePath in filesPaths)
            {
                // First, tries to import as a Pororoca collection
                if (filePath.EndsWith(PororocaCollectionExtension))
                {
                    string fileContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
                    if (TryImportPororocaCollection(fileContent, out var importedPororocaCollection))
                    {
                        mwvm.AddCollection(importedPororocaCollection!);
                    }
                }
                // If not a valid Pororoca collection, then tries to import as a Postman collection
                else if (filePath.EndsWith(PostmanCollectionExtension))
                {
                    string fileContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
                    if (TryImportPostmanCollection(fileContent, out var importedPostmanCollection))
                    {
                        mwvm.AddCollection(importedPostmanCollection!);
                    }
                }
            }
        }
    }

    #endregion

    #region SEARCH CLIENT CERTIFICATE FILES

    internal static Task<string?> SearchClientCertificatePkcs12FileAsync()
    {
        List<FilePickerFileType> fileSelectionfilters = new();
        // Mac OSX file dialogs have problems with file filters... TODO: find if there is a way to solve this
        if (!OperatingSystem.IsMacOS())
        {
            fileSelectionfilters.Add(
                new(Localizer.Instance["RequestAuth/Pkcs12ImportCertificateFileTypesDescription"])
                {
                    Patterns = new List<string> { "*.p12", "*.pfx" }
                }
            );
        }

        FilePickerOpenOptions opts = new()
        {
            Title = Localizer.Instance["RequestAuth/Pkcs12ImportCertificateFileDialogTitle"],
            AllowMultiple = false,
            FileTypeFilter = fileSelectionfilters
        };

        return SelectFileFromStorageAsync(opts);
    }

    internal static Task<string?> SearchClientCertificatePemCertFileAsync()
    {
        List<FilePickerFileType> fileSelectionfilters = new();
        // Mac OSX file dialogs have problems with file filters... TODO: find if there is a way to solve this
        if (!OperatingSystem.IsMacOS())
        {
            fileSelectionfilters.Add(
                new(Localizer.Instance["RequestAuth/PemImportCertificateFileTypesDescription"])
                {
                    Patterns = new List<string> { "*.cer", "*.crt", "*.pem" }
                }
            );
        }

        FilePickerOpenOptions opts = new()
        {
            Title = Localizer.Instance["RequestAuth/PemImportCertificateFileDialogTitle"],
            AllowMultiple = false,
            FileTypeFilter = fileSelectionfilters
        };

        return SelectFileFromStorageAsync(opts);
    }

    internal static Task<string?> SearchClientCertificatePemPrivateKeyFileAsync()
    {
        List<FilePickerFileType> fileSelectionfilters = new();
        // Mac OSX file dialogs have problems with file filters... TODO: find if there is a way to solve this
        if (!OperatingSystem.IsMacOS())
        {
            fileSelectionfilters.Add(
                new(Localizer.Instance["RequestAuth/PemImportPrivateKeyFileTypesDescription"])
                {
                    Patterns = new List<string> { "*.cer", "*.crt", "*.pem", "*.key" }
                }
            );
        }

        FilePickerOpenOptions opts = new()
        {
            Title = Localizer.Instance["RequestAuth/PemImportPrivateKeyFileDialogTitle"],
            AllowMultiple = false,
            FileTypeFilter = fileSelectionfilters
        };

        return SelectFileFromStorageAsync(opts);
    }

    #endregion

    #region GENERIC FILE IMPORT

    internal static Task<string?> SelectFileFromStorageAsync() =>
        SelectFileFromStorageAsync(new());

    internal static async Task<string?> SelectFileFromStorageAsync(FilePickerOpenOptions opts) =>
        (await SelectFilesFromStorageAsync(opts))?.FirstOrDefault();

    private static async Task<List<string>?> SelectFilesFromStorageAsync(FilePickerOpenOptions opts)
    {
        var files = await MainWindow.Instance!.StorageProvider.OpenFilePickerAsync(opts);
        if (files is null)
        {
            return null;
        }
        else
        {
            List<string> filesPaths = new();
            foreach (var file in files!)
            {
                if (file is null || !file.TryGetUri(out var uri))
                {
                    continue;
                }

                // uri.AbsolutePath returns the correct path in Linux
                // TODO: confirm behavior for Windows and MacOSX
                filesPaths.Add(uri.AbsolutePath);
            }

            return filesPaths;
        }
    }

    #endregion

    #region GENERIC FILE EXPORT

    internal static Task<string?> SelectPathForFileToBeSavedAsync(string? suggestedFileName) =>
        SelectPathForFileToBeSavedAsync(new FilePickerSaveOptions() { SuggestedFileName = suggestedFileName });

    private static async Task<string?> SelectPathForFileToBeSavedAsync(FilePickerSaveOptions opts)
    {
        var destFile = await MainWindow.Instance!.StorageProvider.SaveFilePickerAsync(opts);

        if (destFile != null && destFile.TryGetUri(out var uri))
        {
            return uri.ToString();
        }
        else
        {
            return null;
        }
    }

    #endregion
}