using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.ExportCollection;
using Pororoca.Domain.Features.ImportCollection;
using static Pororoca.Desktop.ExportImport.FileExporterImporter;

namespace Pororoca.Desktop.UserData;

public sealed class UserDataManager
{
    private const string appDataProgramFolderName = "Pororoca";
    private const string userDataFolderName = "PororocaUserData";
    private const string userPreferencesFileName = "userPreferences.json";
    private static readonly JsonSerializerOptions userPreferencesJsonOptions = SetupUserPreferencesJsonOptions();

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
        string userPreferencesFilePath = GetUserDataFilePath(userPreferencesFileName);
        if (!File.Exists(userPreferencesFilePath))
        {
            return null;
        }
        else
        {
            try
            {
                var fs = File.Open(userPreferencesFilePath, FileMode.Open, FileAccess.Read);
                return JsonSerializer.Deserialize<UserPreferences>(fs, options: userPreferencesJsonOptions);
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
        string path = GetUserDataFilePath(userPreferencesFileName);
        string json = JsonSerializer.Serialize(userPrefs, options: userPreferencesJsonOptions);
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
        var rootDir = GetUserDataFolder();
        string rootPath = rootDir.FullName;
        return Path.Combine(rootPath, userDataFolderName, fileName);
    }

    private static DirectoryInfo GetUserDataFolder() =>
        /*
            For debugging, the PororocaUserData folder should be located inside the Pororoca.Desktop directory:
                "Pororoca.Desktop\bin\Debug\net6.0"
            
            For a release executable, the PororocaUserData folder location depends on the OS:

            -For Linux, it should be on the same level as the executable.
            Example Linux executable directory: /home/myuser/Programs/MyPororocaDir/
            Then, user data directory will be:  /home/myuser/Programs/MyPororocaDir/PororocaUserData/

            -For MacOS, it should be on the same level as the .app executable (not the same as above)
            Example Mac OSX executable directory: /Applications/Pororoca.app/Contents/MacOS/
            Then, user data directory will be:    /Applications/PororocaUserData/
            This is because that if the user updates the app, the entire Pororoca.app is replaced
            and since it is not intuitive to retrieve content from inside the .app folder,
            the user data folder needs to be outside of the .app.

            -For Windows:
                - If it is a portable executable, it should be on the same level as the executable, just like on Linux.
                - If it is an installed executable, it should be inside the %APPDATA%/Pororoca/ folder. (Roaming folder, not Local or LocalLow)
        */
#if INSTALLED_ON_WINDOWS
        GetUserDataFolderForInstalledOnWindows(); // installed on windows
#elif DEBUG
        GetUserDataFolderForDebug(); // debug, for any OS
#else
        OperatingSystem.IsMacOS() ?
            GetUserDataFolderForMacOSX() : // mac osx
            GetUserDataFolderForPortableExecutable(); // linux and windows portable
#endif


#if DEBUG
    private static DirectoryInfo GetUserDataFolderForDebug()
    {
        // for debug, do not use Environment.ProcessPath, because it returns "C:\Program Files\dotnet";
        // Assembly.Location returns an empty string for single-file apps
        // do not use single-file app on debug
        string currentDirPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location!)!;
        DirectoryInfo currentDir = new(currentDirPath);
        return currentDir.Parent!.Parent!.Parent!;
    }
#endif

    private static DirectoryInfo GetUserDataFolderForMacOSX()
    {
        string currentDirPath = Path.GetDirectoryName(Environment.ProcessPath)!;
        DirectoryInfo currentDir = new(currentDirPath);
        return currentDir.Parent!.Parent!.Parent!;
    }

    private static DirectoryInfo GetUserDataFolderForPortableExecutable()
    {
        string currentDirPath = Path.GetDirectoryName(Environment.ProcessPath)!;
        return new(currentDirPath);
    }

    private static DirectoryInfo GetUserDataFolderForInstalledOnWindows()
    {
        // we are using "AppData\Roaming" path here, to preserve user data
        // when user logs in on another machine of the same corporate network
        string appDataRoamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string currentDirPath = Path.Combine(appDataRoamingPath, appDataProgramFolderName)!;
        return new(currentDirPath);
    }
}