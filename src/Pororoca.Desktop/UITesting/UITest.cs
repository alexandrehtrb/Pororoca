using System.Diagnostics;
using System.Text;
using Pororoca.Desktop.UserData;
using Pororoca.Desktop.ViewModels;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting;

public abstract partial class UITest
{
    public string TestName => GetType().Name;

    public string Log => this.logAppender.ToString();

    public bool? Successful { get; private set; }

    private readonly StringBuilder logAppender;

    private readonly Stopwatch stopwatch;

    public int TotalElapsedSeconds => (int)this.stopwatch.Elapsed.TotalSeconds;

    public TimeSpan ElapsedTime => this.stopwatch.Elapsed;

    public UITest()
    {
        this.logAppender = new();
        this.stopwatch = new();
    }

    public void Assert(bool condition)
    {
        if (condition == false)
        {
            Successful = false;
            throw new UITestException();
        }
    }

    public void Start()
    {
        Successful = null;
        this.stopwatch.Start();
    }

    public void Finish()
    {
        Successful = Successful is null;
        this.stopwatch.Stop();
        Teardown();
    }

    protected virtual void Teardown()
    {
        var mwvm = ((MainWindowViewModel)MainWindow.Instance!.DataContext!);
        mwvm.CollectionsGroupViewDataCtx.CollectionGroupSelectedItem = null;
        mwvm.CollectionsGroupViewDataCtx.Items.Clear();
    }

    public Task Wait(double seconds) => Task.Delay((int)(seconds * 1000));

    protected void AppendToLog(string msg) => this.logAppender.AppendLine(msg);

    public abstract Task RunAsync();

    protected static string GetTestFilesDirPath()
    {
        var userDataDir = UserDataManager.GetUserDataFolder();
        return Path.Combine(userDataDir.FullName, "TestFiles");
    }

    protected static string GetTestFilePath(string subFolder, string fileName) =>
        Path.Combine(GetTestFilesDirPath(), subFolder, fileName);
}

public sealed class UITestException : Exception
{
}