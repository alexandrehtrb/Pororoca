using System.Text;

namespace Pororoca.Desktop.UITesting;

public abstract class UITest
{
    public string TestName => this.GetType().Name;
    
    public string Log => this.logAppender.ToString();

    public bool? Successful { get; private set; }

    private readonly StringBuilder logAppender;

    public UITest() => this.logAppender = new();

    public void Assert(bool condition)
    {
        if (condition == false)
        {
            Successful = false;
            throw new UITestException();
        }
    }

    public void Finish() => Successful ??= true;

    public Task Wait(double seconds) => Task.Delay((int)(seconds * 1000));

    protected void AppendToLog(string msg) => this.logAppender.AppendLine(msg);

    public abstract Task RunAsync();
}

public sealed class UITestException : Exception
{
}