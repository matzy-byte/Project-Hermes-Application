using Json;

public static class DataLogger
{
    public const string PathLoggingFolder = "backend\\logs";
    private static string FullPath { get; set; }

    public static void Initialize()
    {
        //Get the full path
        FullPath = JsonReader.GetFullPath(PathLoggingFolder);

        //Add the File Name
        string time = GetTimeStamp();
        string fileName = $"{time}.log";
        FullPath = Path.Combine(FullPath, fileName);
        FullPath.Replace('\\', '/');

        //Create the file
        File.WriteAllText(FullPath, "Log file created at: " + DateTime.Now.ToString());

        Console.WriteLine(FullPath);
    }

    public static string GetTimeStamp()
    {
        return DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss");         // European date format: dd-MM-yyyy_HH-mm-ss
    }

    public static void AddLog(string message)
    {
        File.AppendAllText(FullPath, Environment.NewLine + GetTimeStamp() + ": " + message);
    }
}