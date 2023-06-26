using System.Text;
using System.Text.RegularExpressions;
using Avalonia.Platform.Storage;
using Pororoca.Desktop.Localization;
using Pororoca.Desktop.ViewModels;
using Pororoca.Desktop.Views;
using static Pororoca.Domain.Features.ExportCollection.PororocaCollectionExporter;
using static Pororoca.Domain.Features.ExportCollection.PostmanCollectionV21Exporter;
using static Pororoca.Domain.Features.ExportEnvironment.PororocaEnvironmentExporter;
using static Pororoca.Domain.Features.ExportEnvironment.PostmanEnvironmentExporter;
using static Pororoca.Domain.Features.ImportCollection.PororocaCollectionImporter;
using static Pororoca.Domain.Features.ImportCollection.PostmanCollectionV21Importer;
using static Pororoca.Domain.Features.ImportEnvironment.PororocaEnvironmentImporter;
using static Pororoca.Domain.Features.ImportEnvironment.PostmanEnvironmentImporter;

namespace Pororoca.Desktop.ExportImport;

internal static partial class FileExporterImporter
{
    internal const string PororocaCollectionExtension = "pororoca_collection.json";
    internal const string PostmanCollectionExtension = "postman_collection.json";
    internal const string PororocaEnvironmentExtension = "pororoca_environment.json";
    internal const string PostmanEnvironmentExtension = "postman_environment.json";

    private const string PororocaCollectionExtensionGlob = $"*.{PororocaCollectionExtension}";
    private const string PostmanCollectionExtensionGlob = $"*.{PostmanCollectionExtension}";
    private const string PororocaEnvironmentExtensionGlob = $"*.{PororocaEnvironmentExtension}";
    private const string PostmanEnvironmentExtensionGlob = $"*.{PostmanEnvironmentExtension}";
    private const string JsonExtensionGlob = $"*.json";

    private static readonly Regex pororocaSchemaRegex = GeneratePororocaSchemaRegex();

    [GeneratedRegex("schema\":\\s*\"Pororoca")]
    private static partial Regex GeneratePororocaSchemaRegex();

    #region EXPORT COLLECTION

    internal static Task ExportCollectionAsync(CollectionViewModel cvm)
    {
        FilePickerSaveOptions opts = new()
        {
            Title = Localizer.Instance.Collection.ExportCollectionDialogTitle,
            FileTypeChoices = new List<FilePickerFileType>()
            {
                new(Localizer.Instance.Collection.PororocaCollectionFormat)
                {
                    Patterns = new List<string> { PororocaCollectionExtensionGlob }
                },
                new(Localizer.Instance.Collection.PostmanCollectionFormat)
                {
                    Patterns = new List<string> { PostmanCollectionExtensionGlob }
                }
            }
        };
        if (OperatingSystem.IsWindows())
        {
            opts.DefaultExtension = PororocaCollectionExtensionGlob;
        }

        return ShowExportCollectionDialogAsync(cvm, opts);
    }

    internal static Task ExportAsPororocaCollectionAsync(CollectionViewModel cvm)
    {
        FilePickerSaveOptions opts = new()
        {
            Title = Localizer.Instance.Collection.ExportAsPororocaCollectionDialogTitle,
            SuggestedFileName = $"{cvm.Name}.{PororocaCollectionExtension}"
        };

        return ShowExportCollectionDialogAsync(cvm, opts);
    }

    internal static Task ExportAsPostmanCollectionAsync(CollectionViewModel cvm)
    {
        FilePickerSaveOptions opts = new()
        {
            Title = Localizer.Instance.Collection.ExportAsPostmanCollectionDialogTitle,
            SuggestedFileName = $"{cvm.Name}.{PostmanCollectionExtension}"
        };

        return ShowExportCollectionDialogAsync(cvm, opts);
    }

    private static async Task ShowExportCollectionDialogAsync(CollectionViewModel cvm, FilePickerSaveOptions opts)
    {
        string? destFilePath = await SelectPathForFileToBeSavedAsync(opts);

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
            Title = Localizer.Instance.Environment.ExportEnvironmentDialogTitle,
            FileTypeChoices = new List<FilePickerFileType>()
            {
                new(Localizer.Instance.Environment.PororocaEnvironmentFormat)
                {
                    Patterns = new List<string> { PororocaEnvironmentExtensionGlob }
                },
                new(Localizer.Instance.Environment.PostmanEnvironmentFormat)
                {
                    Patterns = new List<string> { PostmanEnvironmentExtensionGlob }
                }
            }
        };
        if (OperatingSystem.IsWindows())
        {
            opts.DefaultExtension = PororocaEnvironmentExtensionGlob;
        }

        return ShowExportEnvironmentDialogAsync(evm, opts);
    }

    internal static Task ExportAsPororocaEnvironmentAsync(EnvironmentViewModel evm)
    {
        FilePickerSaveOptions opts = new()
        {
            Title = Localizer.Instance.Environment.ExportAsPororocaEnvironmentDialogTitle,
            SuggestedFileName = $"{evm.Name}.{PororocaEnvironmentExtension}"
        };

        return ShowExportEnvironmentDialogAsync(evm, opts);
    }

    internal static Task ExportAsPostmanEnvironmentAsync(EnvironmentViewModel evm)
    {
        FilePickerSaveOptions opts = new()
        {
            Title = Localizer.Instance.Environment.ExportAsPostmanEnvironmentDialogTitle,
            SuggestedFileName = $"{evm.Name}.{PostmanEnvironmentExtension}"
        };

        return ShowExportEnvironmentDialogAsync(evm, opts);
    }

    private static async Task ShowExportEnvironmentDialogAsync(EnvironmentViewModel evm, FilePickerSaveOptions opts)
    {
        string? destFilePath = await SelectPathForFileToBeSavedAsync(opts);

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
                new(Localizer.Instance.Collection.ImportEnvironmentDialogTypes)
                {
                    Patterns = new List<string> { PororocaEnvironmentExtensionGlob, PostmanEnvironmentExtensionGlob, JsonExtensionGlob }
                }
            );
        }

        FilePickerOpenOptions opts = new()
        {
            Title = Localizer.Instance.Collection.ImportEnvironmentDialogTitle,
            AllowMultiple = true,
            FileTypeFilter = fileSelectionfilters
        };

        var filesPaths = await SelectFilesFromStorageAsync(opts);
        if (filesPaths != null)
        {
            foreach (string filePath in filesPaths)
            {
                string fileContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8) ?? string.Empty;
                bool isPororocaEnvironment = pororocaSchemaRegex.IsMatch(fileContent);

                // First, tries to import as a Pororoca environment
                if (isPororocaEnvironment)
                {
                    if (TryImportPororocaEnvironment(fileContent, out var importedPororocaEnvironment))
                    {
                        importedPororocaEnvironment!.IsCurrent = false; // Imported environment should always be disabled
                        egvm.AddEnvironment(importedPororocaEnvironment!);
                    }
                }
                // If not a valid Pororoca environment, then tries to import as a Postman environment
                else
                {
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
                new(Localizer.Instance.Collection.ImportCollectionDialogTypes)
                {
                    Patterns = new List<string> { PororocaCollectionExtensionGlob, PostmanCollectionExtensionGlob, JsonExtensionGlob }
                }
            );
        }

        FilePickerOpenOptions opts = new()
        {
            Title = Localizer.Instance.Collection.ImportCollectionDialogTitle,
            AllowMultiple = true,
            FileTypeFilter = fileSelectionfilters
        };
        var filesPaths = await SelectFilesFromStorageAsync(opts);
        if (filesPaths != null)
        {
            foreach (string filePath in filesPaths)
            {
                string fileContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8) ?? string.Empty;
                bool isPororocaCollection = pororocaSchemaRegex.IsMatch(fileContent);

                // First, tries to import as a Pororoca collection
                if (isPororocaCollection)
                {
                    if (TryImportPororocaCollection(fileContent, preserveId: false, out var importedPororocaCollection))
                    {
                        mwvm.AddCollection(importedPororocaCollection!);
                    }
                }
                // If not a valid Pororoca collection, then tries to import as a Postman collection
                else
                {
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
                new(Localizer.Instance.RequestAuth.Pkcs12ImportCertificateFileTypesDescription)
                {
                    Patterns = new List<string> { "*.p12", "*.pfx" }
                }
            );
        }

        FilePickerOpenOptions opts = new()
        {
            Title = Localizer.Instance.RequestAuth.Pkcs12ImportCertificateFileDialogTitle,
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
                new(Localizer.Instance.RequestAuth.PemImportCertificateFileTypesDescription)
                {
                    Patterns = new List<string> { "*.cer", "*.crt", "*.pem" }
                }
            );
        }

        FilePickerOpenOptions opts = new()
        {
            Title = Localizer.Instance.RequestAuth.PemImportCertificateFileDialogTitle,
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
                new(Localizer.Instance.RequestAuth.PemImportPrivateKeyFileTypesDescription)
                {
                    Patterns = new List<string> { "*.cer", "*.crt", "*.pem", "*.key" }
                }
            );
        }

        FilePickerOpenOptions opts = new()
        {
            Title = Localizer.Instance.RequestAuth.PemImportPrivateKeyFileDialogTitle,
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
                if (file is null || file.Path is null)
                {
                    continue;
                }

                // uri.LocalPath returns the correct path in Linux and Windows
                // careful with file paths with whitespaces in them
                // TODO: confirm behavior for MacOSX
                filesPaths.Add(file.Path.LocalPath);
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

        if (destFile != null && destFile.Path is not null)
        {
            // uri.LocalPath returns the correct path in Linux and Windows
            // careful with file paths with whitespaces in them
            // TODO: confirm behavior for MacOSX
            return destFile.Path.LocalPath;
        }
        else
        {
            return null;
        }
    }

    #endregion
}