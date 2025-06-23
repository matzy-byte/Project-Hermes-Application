using System.Collections.Concurrent;
using Json;

public static class DataLogger
{
    public const string PathLoggingFolder = "backend\\logs";
    private static string FullPath { get; set; }
    private static readonly object fileLock = new object();
    private static ConcurrentQueue<string> BufferedLogs { get; set; } = new();

    /// <summary>
    /// Initialize the Data logger (create file)
    /// </summary>
    public static void Initialize()
    {
        FullPath = JsonReader.GetFullPath(PathLoggingFolder);

        if (!Directory.Exists(FullPath))
            Directory.CreateDirectory(FullPath);

        string time = GetTimeStamp();
        string fileName = $"{time}.log";
        FullPath = Path.Combine(FullPath, fileName);
        FullPath = FullPath.Replace('\\', '/');

        lock (fileLock)
        {
            File.WriteAllText(FullPath, "Log file created at: " + DateTime.Now.ToString());
        }

        Console.WriteLine(FullPath);
    }

    /// <summary>
    /// Get The time stamp in dd-mm-hh-mm-ss
    /// </summary>
    public static string GetTimeStamp()
    {
        return DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss");
    }

    /// <summary>
    /// Add Log Message to Log File
    /// </summary>
    public static void AddLog(string message)
    {
        string logEntry = Environment.NewLine + GetTimeStamp() + ": " + message;
        BufferedLogs.Enqueue(GetTimeStamp() + ": " + message);

        lock (fileLock)
        {
            try
            {
                File.AppendAllText(FullPath, logEntry);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Logging failed: " + e.Message);
            }
        }
    }

    /// <summary>
    /// Gather and clear log buffer
    /// </summary>
    public static List<string> CollectLog()
    {
        List<string> logs = [];
        while (BufferedLogs.TryDequeue(out var log))
        {
            logs.Add(log);
        }
        return logs;
    }
}
