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
using static Pororoca.Domain.Features.ImportCollection.OpenApiImporter;
using static Pororoca.Domain.Features.ImportCollection.PororocaCollectionImporter;
using static Pororoca.Domain.Features.ImportCollection.PostmanCollectionV21Importer;
using static Pororoca.Domain.Features.ImportCollection.InsomniaCollectionV4Importer;
using static Pororoca.Domain.Features.ImportEnvironment.PororocaEnvironmentImporter;
using static Pororoca.Domain.Features.ImportEnvironment.PostmanEnvironmentImporter;
using Pororoca.Desktop.Converters;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Desktop.HotKeys;
using MsBox.Avalonia.Enums;

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
    private const string YamlExtensionGlob = $"*.yaml";
    private const string YmlExtensionGlob = $"*.yml";

    private static readonly Regex pororocaSchemaRegex = GeneratePororocaSchemaRegex();

    [GeneratedRegex("schema\":\\s*\"Pororoca")]
    private static partial Regex GeneratePororocaSchemaRegex();

    // Postman exported files use UTF-8 without BOM
    // Insomnia can't read Postman collection and environment files
    // if they are in UTF-8 encoding with BOM
    private static readonly UTF8Encoding utf8EncodingWithoutBOM = new(encoderShouldEmitUTF8Identifier: false);

    #region EXPORT COLLECTION

    internal static async Task ShowExportCollectionToFileDialogAsync(PororocaCollection col, ExportCollectionFormat format)
    {
        FilePickerSaveOptions opts = new()
        {
            Title = Localizer.Instance.ExportCollection.DialogTitle,
            SuggestedFileName = $"{col.Name}.{(format switch
            {
                ExportCollectionFormat.Pororoca => PororocaCollectionExtension,
                ExportCollectionFormat.Postman => PostmanCollectionExtension,
                _ => string.Empty
            })}"
        };

        if (!OperatingSystem.IsMacOS())
        {
            var fileTypeChoices = new List<FilePickerFileType>();
            fileTypeChoices.Add(format switch
            {
                ExportCollectionFormat.Pororoca => new(Localizer.Instance.ExportCollection.PororocaCollectionFormat)
                {
                    Patterns = new List<string> { PororocaCollectionExtensionGlob }
                },
                ExportCollectionFormat.Postman => new(Localizer.Instance.ExportCollection.PostmanCollectionFormat)
                {
                    Patterns = new List<string> { PostmanCollectionExtensionGlob }
                },
                _ => new(string.Empty)
            });
            opts.FileTypeChoices = fileTypeChoices;

            if (OperatingSystem.IsWindows())
            {
                opts.DefaultExtension = PororocaCollectionExtensionGlob;
            }
        }

        string? destFilePath = await SelectPathForFileToBeSavedAsync(opts);

        if (destFilePath != null)
        {
            string json = format switch
            {
                ExportCollectionFormat.Pororoca => ExportAsPororocaCollection(col),
                ExportCollectionFormat.Postman => ExportAsPostmanCollectionV21(col),
                _ => string.Empty
            };
            var encoding = format switch
            {
                ExportCollectionFormat.Pororoca => Encoding.UTF8,
                ExportCollectionFormat.Postman => utf8EncodingWithoutBOM,
                _ => Encoding.UTF8
            };
            await File.WriteAllTextAsync(destFilePath, json, encoding);
        }
    }

    #endregion

    #region EXPORT ENVIRONMENT

    internal static async Task ShowExportEnvironmentToFileDialogAsync(PororocaEnvironment env, ExportEnvironmentFormat format)
    {
        FilePickerSaveOptions opts = new()
        {
            Title = Localizer.Instance.ExportEnvironment.DialogTitle,
            SuggestedFileName = $"{env.Name}.{(format switch
            {
                ExportEnvironmentFormat.Pororoca => PororocaEnvironmentExtension,
                ExportEnvironmentFormat.Postman => PostmanEnvironmentExtension,
                _ => string.Empty
            })}"
        };

        if (!OperatingSystem.IsMacOS())
        {
            var fileTypeChoices = new List<FilePickerFileType>();
            fileTypeChoices.Add(format switch
            {
                ExportEnvironmentFormat.Pororoca => new(Localizer.Instance.ExportEnvironment.PororocaEnvironmentFormat)
                {
                    Patterns = new List<string> { PororocaEnvironmentExtensionGlob }
                },
                ExportEnvironmentFormat.Postman => new(Localizer.Instance.ExportEnvironment.PostmanEnvironmentFormat)
                {
                    Patterns = new List<string> { PostmanEnvironmentExtensionGlob }
                },
                _ => new(string.Empty)
            });
            opts.FileTypeChoices = fileTypeChoices;

            if (OperatingSystem.IsWindows())
            {
                opts.DefaultExtension = PororocaEnvironmentExtensionGlob;
            }
        }

        string? destFilePath = await SelectPathForFileToBeSavedAsync(opts);

        if (destFilePath != null)
        {
            string json = format switch
            {
                ExportEnvironmentFormat.Pororoca => ExportAsPororocaEnvironment(env),
                ExportEnvironmentFormat.Postman => ExportAsPostmanEnvironment(env),
                _ => string.Empty
            };
            var encoding = format switch
            {
                ExportEnvironmentFormat.Pororoca => Encoding.UTF8,
                ExportEnvironmentFormat.Postman => utf8EncodingWithoutBOM,
                _ => Encoding.UTF8
            };
            await File.WriteAllTextAsync(destFilePath, json, encoding);
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
                if (isPororocaEnvironment && TryImportPororocaEnvironment(fileContent, out var importedPororocaEnvironment))
                {
                    egvm.AddEnvironment(importedPororocaEnvironment!);
                }
                // If not a valid Pororoca environment, then tries to import as a Postman environment
                else if (TryImportPostmanEnvironment(fileContent, out var importedPostmanEnvironment))
                {
                    egvm.AddEnvironment(importedPostmanEnvironment!);
                }
                else
                {
                    Dialogs.ShowDialog(
                        title: Localizer.Instance.Collection.FailedToImportEnvironmentDialogTitle,
                        message: Localizer.Instance.Collection.FailedToImportEnvironmentDialogMessage,
                        buttons: ButtonEnum.Ok);
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
                    Patterns = new List<string> { PororocaCollectionExtensionGlob, PostmanCollectionExtensionGlob, JsonExtensionGlob, YamlExtensionGlob, YmlExtensionGlob }
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
                bool possiblyPororocaCollection = pororocaSchemaRegex.IsMatch(fileContent);
                bool possiblyPostmanCollection = fileContent.Contains("postman");
                bool possiblyOpenApi = fileContent.Contains("openapi") && (filePath.EndsWith(".yaml") || filePath.EndsWith(".yml") || filePath.EndsWith(".json"));
                bool possiblyInsomnia = fileContent.Contains("insomnia") && filePath.EndsWith(".json");

                // First, tries to import as a Pororoca collection
                if (possiblyPororocaCollection && TryImportPororocaCollection(fileContent, preserveId: false, out var importedPororocaCollection))
                {
                    mwvm.AddCollection(importedPororocaCollection!);
                }
                // If not a valid Pororoca collection, then tries to import as a Postman collection
                else if (possiblyPostmanCollection && TryImportPostmanCollection(fileContent, out var importedPostmanCollection))
                {
                    mwvm.AddCollection(importedPostmanCollection!);
                }
                else if (possiblyOpenApi && TryImportOpenApi(fileContent, out var importedOpenApiCollection))
                {
                    mwvm.AddCollection(importedOpenApiCollection!);
                }
                else if (possiblyInsomnia && TryImportInsomniaCollection(fileContent, out var importedInsomniaCollection))
                {
                    mwvm.AddCollection(importedInsomniaCollection!);
                }
                else
                {
                    Dialogs.ShowDialog(
                        title: Localizer.Instance.Collection.FailedToImportCollectionDialogTitle,
                        message: Localizer.Instance.Collection.FailedToImportCollectionDialogMessage,
                        buttons: ButtonEnum.Ok);
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

    internal static async Task<string?> SelectFolderAsync()
    {
        FolderPickerOpenOptions options = new()
        {
            AllowMultiple = false,
            Title = Localizer.Instance.SelectFolderDialog.Title
        };

        var selectedFolders = await MainWindow.Instance!.StorageProvider.OpenFolderPickerAsync(options);

        var selectedFolder = selectedFolders.Count > 0 ? selectedFolders[0] : null;

        // uri.LocalPath returns the correct path in Linux and Windows
        // careful with file paths with whitespaces in them
        // TODO: confirm behavior for MacOSX
        return selectedFolder?.Path?.LocalPath;
    }

    #endregion
}