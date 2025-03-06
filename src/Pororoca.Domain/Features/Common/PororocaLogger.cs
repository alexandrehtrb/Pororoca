using System.Text;

namespace Pororoca.Domain.Features.Common;

public enum PororocaLogLevel
{
    Information = 1,
    Warning = 2,
    Error = 3,
    Fatal = 4
}

public sealed class PororocaLogger
{
    public static PororocaLogger? Instance;

    private readonly string baseDirPath;

    public PororocaLogger(DirectoryInfo userDataDir)
    {
        this.baseDirPath = Path.Combine(userDataDir.FullName, "Logs");
        if (!Directory.Exists(this.baseDirPath))
        {
            Directory.CreateDirectory(this.baseDirPath);
        }
    }

    public void Log(PororocaLogLevel level, string message, Exception? ex = null)
    {
        try
        {
            DateTime now = DateTime.Now;
            string fileName = $"{now:yyyy-MM-dd}.log";
            string filePath = Path.Combine(this.baseDirPath, fileName);
            StringBuilder sb = new();
            sb.Append('#', 20);
            sb.AppendLine();
            sb.AppendLine($"Time: {now:HH:mm:ss}");
            sb.AppendLine($"Severity: {level}");
            sb.AppendLine($"Message: {message}");
            if (ex is not null)
            {
                sb.AppendLine($"Exception: {ex.ToString()}");
            }
            sb.Append('#', 20);
            sb.AppendLine();
            File.AppendAllText(filePath, sb.ToString());
        }
        catch
        {
            // Who logs the logger's errors?
        }
    }
}