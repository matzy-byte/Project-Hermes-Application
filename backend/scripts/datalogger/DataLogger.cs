using System;
using System.IO;
using System.Threading;
using Json;

public static class DataLogger
{
    public const string PathLoggingFolder = "backend\\logs";
    private static string FullPath { get; set; }
    private static readonly object fileLock = new object();

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

    public static string GetTimeStamp()
    {
        return DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss");
    }

    public static void AddLog(string message)
    {
        string logEntry = Environment.NewLine + GetTimeStamp() + ": " + message;

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
}
