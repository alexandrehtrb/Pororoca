using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.ExportCollection;
using Pororoca.Domain.Features.ImportCollection;
using static Pororoca.Domain.Features.Common.AvailablePororocaRequestSelectionOptions;

namespace Pororoca.Desktop.UserData;

public sealed class UserDataManager
{
    private const string UserDataFolderName = "PororocaUserData";
    private const string UserPreferencesFileName = "userPreferences.json";
    private static readonly JsonSerializerOptions UserPreferencesJsonOptions = SetupUserPreferencesJsonOptions();

    private static JsonSerializerOptions SetupUserPreferencesJsonOptions()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.WriteIndented = true;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        return options;
    }

    public static UserPreferences? LoadUserPreferences()
    {
        string userPreferencesFilePath = GetUserDataFilePath(UserPreferencesFileName);
        if (!File.Exists(userPreferencesFilePath))
        {
            return null;
        }
        else
        {
            try
            {
                var fs = File.Open(userPreferencesFilePath, FileMode.Open, FileAccess.Read);
                return JsonSerializer.Deserialize<UserPreferences>(fs, options: UserPreferencesJsonOptions);
            }
            catch
            {
                return null;
            }
        }
    }

    public static PororocaCollection[] LoadUserCollections()
    {
        string userDataFolderPath = GetUserDataFilePath(string.Empty);
        DirectoryInfo userDataFolder = new(userDataFolderPath);
        if (!userDataFolder.Exists)
        {
            return Array.Empty<PororocaCollection>();
        }
        else
        {
            return userDataFolder
                .GetFiles()
                .Where(f => f.FullName.EndsWith(PororocaCollectionExtension))
                .Select(f =>
                {
                    try
                    {
                        string json = File.ReadAllText(f.FullName, Encoding.UTF8);
                        if (PororocaCollectionImporter.TryImportPororocaCollection(json, out var col))
                        {
                            return col;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    catch
                    {
                        return null;
                    }
                })
                .Where(c => c != null)
                .Cast<PororocaCollection>()
                .ToArray();
        }
    }

    public static Task SaveUserData(UserPreferences userPrefs, IEnumerable<PororocaCollection> collections)
    {
        CreateUserDataFolderIfNotExists();
        DeleteUserDataFiles();
        return Task.WhenAll(new[]
        {
            SaveUserPreferences(userPrefs),
            SaveUserCollections(collections)
        });
    }

    private static Task SaveUserPreferences(UserPreferences userPrefs)
    {
        string path = GetUserDataFilePath(UserPreferencesFileName);
        string json = JsonSerializer.Serialize(userPrefs, options: UserPreferencesJsonOptions);
        return File.WriteAllTextAsync(path, json, Encoding.UTF8);
    }

    private static Task SaveUserCollections(IEnumerable<PororocaCollection> collections)
    {
        List<Task> savingTasks = new();
        foreach (var col in collections)
        {
            string path = GetUserDataFilePath($"{col.Id}.{PororocaCollectionExtension}");
            string json = PororocaCollectionExporter.ExportAsPororocaCollection(col, false);
            savingTasks.Add(File.WriteAllTextAsync(path, json, Encoding.UTF8));
        }
        return Task.WhenAll(savingTasks);
    }

    private static void CreateUserDataFolderIfNotExists()
    {
        string folderPath = GetUserDataFilePath(string.Empty);
        DirectoryInfo di = new(folderPath);
        if (!di.Exists)
        {
            di.Create();
        }
    }

    private static void DeleteUserDataFiles()
    {
        string folderPath = GetUserDataFilePath(string.Empty);
        DirectoryInfo di = new(folderPath);
        foreach (var fi in di.GetFiles())
        {
            fi.Delete();
        }
    }

    public static string GetUserDataFilePath(string fileName)
    {
        // Example Mac OSX executable directory: /Applications/Pororoca.app/Contents/MacOS/
        // Then, user data directory will be:    /Applications/PororocaUserData/
        // This is because that if the user updates the app, the entire Pororoca.app is replaced
        // and since it is not intuitive to retrieve content from inside the .app folder,
        // the user data folder needs to be outside of the .app.        

        // Example Windows executable directory: C:\Users\you\Desktop\Pororoca\
        // Then, user data directory will be:    C:\Users\you\Desktop\Pororoca\PororocaUserData\

        // If debugging from VS Code, do not use Environment.ProcessPath, because it returns "C:\Program Files\dotnet"

#if DEBUG
        string currentDirPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location!)!;
        DirectoryInfo currentDir = new(currentDirPath);
        var rootDir = currentDir.Parent!.Parent!.Parent!; // Pororoca.Desktop\bin\Debug\net6.0
#else
        string currentDirPath = Path.GetDirectoryName(Environment.ProcessPath)!;
        DirectoryInfo currentDir = new(currentDirPath);
        var rootDir = OperatingSystem.IsMacOS() ?
                      currentDir.Parent!.Parent!.Parent! :
                      currentDir;
#endif
        string rootPath = rootDir.FullName;
        return Path.Combine(rootPath, UserDataFolderName, fileName);
    }
}