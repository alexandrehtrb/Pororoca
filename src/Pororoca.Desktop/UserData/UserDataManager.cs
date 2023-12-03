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

public static class UserDataManager
{
    private const string appDataProgramFolderName = "Pororoca";
    private const string userDataFolderName = "PororocaUserData";
    private const string userPreferencesFileName = "userPreferences.json";

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
                string json = File.ReadAllText(userPreferencesFilePath, Encoding.UTF8);
                return JsonSerializer.Deserialize(json, UserPreferencesJsonSrcGenContext.Default.UserPreferences);
            }
            catch
            {
                return null;
            }
        }
    }

    public static PororocaCollection[] LoadUserCollections() =>
        FetchSavedUserCollectionsFiles()
        .Select(f =>
        {
            try
            {
                string json = File.ReadAllText(f.FullName, Encoding.UTF8);
                if (PororocaCollectionImporter.TryImportPororocaCollection(json, preserveId: true, out var col))
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

    private static IEnumerable<FileInfo> FetchSavedUserCollectionsFiles()
    {
        string userDataFolderPath = GetUserDataFilePath(string.Empty);
        DirectoryInfo userDataFolder = new(userDataFolderPath);
        if (!userDataFolder.Exists)
        {
            return Array.Empty<FileInfo>();
        }
        else
        {
            return userDataFolder
                .GetFiles()
                .Where(f => f.FullName.EndsWith(PororocaCollectionExtension));
        }
    }

    public static void SaveUserData(UserPreferences userPrefs, IEnumerable<PororocaCollection> collections)
    {
        CreateUserDataFolderIfNotExists();
        SaveUserPreferences(userPrefs);
        SaveUserCollections(collections);
    }

    private static void SaveUserPreferences(UserPreferences userPrefs)
    {
        string path = GetUserDataFilePath(userPreferencesFileName);
        string json = JsonSerializer.Serialize(userPrefs, UserPreferencesJsonSrcGenContext.Default.UserPreferences);
        File.WriteAllText(path, json, Encoding.UTF8);
    }

    private static void SaveUserCollections(IEnumerable<PororocaCollection> collections)
    {
        List<Guid> savedColsIds;

        try
        {
            savedColsIds = FetchSavedUserCollectionsFiles()
                .Select(c => Guid.Parse(Path.GetFileName(c.FullName).Replace($".{PororocaCollectionExtension}", string.Empty)))
                .ToList();
        }
        catch
        {
            savedColsIds = new();
        }

        foreach (var col in collections)
        {
            string path = GetUserDataFilePath($"{col.Id}.{PororocaCollectionExtension}");
            string json = PororocaCollectionExporter.ExportAsPororocaCollection(col, false);

            File.WriteAllText(path, json, Encoding.UTF8);
            // Marking collections that were deleted, to delete their files
            savedColsIds.Remove(col.Id);
        }

        // The collections found in the folder that do not exist anymore will be deleted
        foreach (var savedColId in savedColsIds)
        {
            string path = GetUserDataFilePath($"{savedColId}.{PororocaCollectionExtension}");
            File.Delete(path);
        }
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

    public static string GetUserDataFilePath(string fileName)
    {
        var rootDir = GetUserDataFolder();
        string rootPath = rootDir.FullName;
        return Path.Combine(rootPath, userDataFolderName, fileName);
    }

    internal static DirectoryInfo GetUserDataFolder() =>
        /*
            For debugging, the PororocaUserData folder should be located inside the Pororoca.Desktop directory:
                "Pororoca.Desktop\bin\Debug\net8.0\win-x64\"
            
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
        // .NET 7 no longer has runtime identifer divided Debug folder
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