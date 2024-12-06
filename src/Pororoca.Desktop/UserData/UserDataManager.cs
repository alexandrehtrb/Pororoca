using System.Reflection;
using System.Text;
using System.Text.Json;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.ExportCollection;
using Pororoca.Domain.Features.ImportCollection;
using static Pororoca.Desktop.ExportImport.FileExporterImporter;

namespace Pororoca.Desktop.UserData;

public static class UserDataManager
{
    private const string appProgramFolderName = "Pororoca";
    private const string userDataFolderName = "PororocaUserData";
    private const string userPreferencesFileName = "userPreferences.json";

    public static UserPreferences? LoadUserPreferences()
    {
        string userPreferencesFilePath = GetUserDataFilePath(userPreferencesFileName);
        if (!File.Exists(userPreferencesFilePath))
        {
            if (NeedsMacOSXUserDataFolderMigrationToV3())
            {
                // this is to load an old user preferences file for MacOSX,
                // in case a migration needs to happen
                userPreferencesFilePath = Path.Combine(GetUserDataFolderForMacOSX_BeforePororocaV3().FullName, userPreferencesFileName);
                if (!File.Exists(userPreferencesFilePath))
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        try
        {
            using FileStream fs = new(userPreferencesFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize: 128, useAsync: false);
            var userPrefs = JsonSerializer.Deserialize(fs, UserPreferencesJsonSrcGenContext.Default.UserPreferences);
            return userPrefs;
        }
        catch
        {
            return null;
        }
    }

    public static async IAsyncEnumerable<PororocaCollection> LoadUserCollectionsAsync()
    {
        var files = FetchSavedUserCollectionsFiles();
        foreach (var file in files)
        {
            var col = await LoadUserCollectionAsync(file);
            if (col is not null)
            {
                yield return col;
            }
        }
    }

    private static async Task<PororocaCollection?> LoadUserCollectionAsync(FileInfo fi)
    {
        try
        {
            using FileStream fs = new(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize: 16384, useAsync: true);
            var col = await PororocaCollectionImporter.ImportPororocaCollectionAsync(fs, preserveId: true);
            // await above is necessary because of the "using" on the FileStream
            return col;
        }
        catch
        {
            return null;
        }
    }

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
                .Where(f => f.FullName.EndsWith(PororocaCollectionExtension)
                        && (f.Name.StartsWith("deleted_") == false));
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

            using FileStream fs = new(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite, bufferSize: 16384, useAsync: false);
            PororocaCollectionExporter.ExportAsPororocaCollection(fs, col);

            // Marking collections that were deleted, to delete their files
            savedColsIds.Remove(col.Id);
        }

        // The collections found in the folder that do not exist anymore will be soft-deleted
        foreach (var savedColId in savedColsIds)
        {
            // Executing soft delete for safety
            string originalPath = GetUserDataFilePath($"{savedColId}.{PororocaCollectionExtension}");
            string renamedPath = GetUserDataFilePath($"deleted_{savedColId}.{PororocaCollectionExtension}");
            File.Move(originalPath, renamedPath);
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

    // CAREFUL!!! dotnet format may cut parts of this method that should not be removed!!!
    internal static bool NeedsMacOSXUserDataFolderMigrationToV3() =>
        OperatingSystem.IsMacOS() && GetUserDataFolderForMacOSX_BeforePororocaV3().Exists;

    internal static void ExecuteMacOSXUserDataFolderMigrationToV3()
    {
        try
        {
            if (OperatingSystem.IsMacOS() == false)
                return;

            var oldDir = GetUserDataFolderForMacOSX_BeforePororocaV3();
            if (oldDir.Exists == false)
                return;

            var currentDir = GetUserDataFolderForMacOSX();
            if (currentDir.Exists == false)
            {
                currentDir.Create();
            }

            foreach (var fi in oldDir.EnumerateFiles())
            {
                string destFileName = Path.Combine(currentDir.FullName, fi.Name);
                fi.CopyTo(destFileName, overwrite: true);
            }

            // let's rename the old dir, so this migration won't be required every time
            // if the user forgets to delete the old dir
            string oldDirRenamedNewPath = Path.Combine(oldDir.Parent!.FullName, "PororocaUserData_old_backup");
            oldDir.MoveTo(oldDirRenamedNewPath);
        }
        catch
        {
        }
    }

    #region GET USER DATA DIRECTORY PATH

    public static string GetUserDataFilePath(string fileName)
    {
        var rootDir = GetUserDataFolder();
        string rootPath = rootDir.FullName;
        return Path.Combine(rootPath, fileName);
    }

    internal static DirectoryInfo GetUserDataFolder() =>
        /*
            - For debugging, the PororocaUserData folder should be located inside the Pororoca.Desktop directory:
                C:\MyProjects\Pororoca\src\Pororoca.Desktop\PororocaUserData\

            - For a release executable, the PororocaUserData folder location depends on the OS:

            - For Linux, it should be on the same level as the executable.
                Example Linux executable directory: /home/myuser/Programs/MyPororocaDir/
                Then, user data directory will be:  /home/myuser/Programs/MyPororocaDir/PororocaUserData/

            - For installed on Debian-related distros:
                /home/myuser/.config/Pororoca/PororocaUserData

            - For MacOS, it should be inside the Application Support directory:
                /Users/myuser/Library/Application Support/Pororoca/PororocaUserData

            - For Windows:
                - If it is a portable executable, it should be on the same level as the executable, just like on Linux.
                - If it is an installed executable, it should be inside the %APPDATA%/Pororoca/ folder. (Roaming folder, not Local or LocalLow)

            - CAREFUL!!! dotnet format may cut parts of this method that should not be removed!!!
        */
#if INSTALLED_ON_WINDOWS
        GetUserDataFolderForInstalledOnWindows(); // installed on windows
#elif INSTALLED_ON_DEBIAN
        GetUserDataFolderForInstalledOnDebian(); // installed on debian / ubuntu
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
        var pororocaDesktopSrcDir = currentDir.Parent!.Parent!.Parent!;
        return new(Path.Combine(pororocaDesktopSrcDir.FullName, userDataFolderName));
    }
#endif

    private static DirectoryInfo GetUserDataFolderForMacOSX_BeforePororocaV3()
    {
        string currentDirPath = Path.GetDirectoryName(Environment.ProcessPath)!;
        DirectoryInfo currentDir = new(currentDirPath);
        var applicationsDir = currentDir.Parent!.Parent!.Parent!;
        return new(Path.Combine(applicationsDir.FullName, userDataFolderName));
    }

    private static DirectoryInfo GetUserDataFolderForMacOSX()
    {
        string applicationsSupportDirPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return new(Path.Combine(applicationsSupportDirPath, appProgramFolderName, userDataFolderName));
    }

    private static DirectoryInfo GetUserDataFolderForPortableExecutable()
    {
        string currentDirPath = Path.GetDirectoryName(Environment.ProcessPath)!;
        return new(Path.Combine(currentDirPath, userDataFolderName));
    }

    private static DirectoryInfo GetUserDataFolderForInstalledOnWindows()
    {
        // we are using "AppData\Roaming" path here, to preserve user data
        // when user logs in on another machine of the same corporate network
        string appDataRoamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string userDataDirPath = Path.Combine(appDataRoamingPath, appProgramFolderName, userDataFolderName)!;
        return new(userDataDirPath);
    }

    private static DirectoryInfo GetUserDataFolderForInstalledOnDebian()
    {
        // we are using "/home/myuser/.config/" path here, to preserve user data
        // when user logs in on another machine of the same corporate network
        string userConfigPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string userDataDirPath = Path.Combine(userConfigPath, appProgramFolderName, userDataFolderName)!;
        return new(userDataDirPath);
    }

    #endregion
}