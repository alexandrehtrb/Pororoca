using AlexandreHtrb.AvaloniaUITest;
using Pororoca.Desktop.UserData;
using Pororoca.Desktop.ViewModels;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting;

public abstract class PororocaUITest : UITest
{
    protected MainWindowViewModel MainWindowVm => ((MainWindowViewModel)MainWindow.Instance!.DataContext!);

    protected override void Teardown()
    {
        MainWindowVm.CollectionsGroupViewDataCtx.CollectionGroupSelectedItem = null;
        MainWindowVm.CollectionsGroupViewDataCtx.Items.Clear();
    }

    internal static string GetTestFilesDirPath()
    {
        var userDataDir = UserDataManager.GetUserDataFolder();
        return Path.Combine(userDataDir.FullName, "TestFiles");
    }

    protected static string GetTestFilePath(string subFolder, string fileName) =>
        Path.Combine(GetTestFilesDirPath(), subFolder, fileName);
}