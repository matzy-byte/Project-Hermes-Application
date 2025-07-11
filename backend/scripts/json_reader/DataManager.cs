using Helper;
using Logs;

namespace Json;

public static class DataManager
{
    public static List<TransitInfoWrapper> AllTransits { get; set; } = [];
    public static List<LineWrapper> AllLines { get; set; } = [];
    public static List<Station> AllStations = [];

    // Paths to the json files
    private const string FILESTATIONJSON = "haltestellen_v2.json";
    private const string FILELINEJSON = "lines_v2.json";
    private const string FILEGEODATA = "KVVLinesGeoJSON.json";
    private const string FILETRANSITINFO = "KVV_Transit_Information.json";

    /// <summary>
    /// Loads all necessary data from JSON files into memory.
    /// </summary>
    public static void LoadDataFromJson()
    {
        Console.WriteLine("Start Extracting Data from JSON Files ...");

        AllStations = JsonReader.LoadListedData<Station, StationWrapper>(FILESTATIONJSON, x => new Station(x));
        DataLogger.AddLog("Number of Stations Loaded: " + AllStations.Count);

        AllLines = JsonReader.LoadNestedListData<RootObjectWrapper, LineWrapper>(FILELINEJSON, x => x.Lines);
        DataLogger.AddLog("Number of Lines Loaded: " + AllLines.Count);

        AllTransits = JsonReader.LoadData<List<TransitInfoWrapper>>(FILETRANSITINFO);
        DataLogger.AddLog("Number of Transits Loaded: " + AllLines.Count);

        Console.WriteLine($"Number Of Stations: {AllStations.Count}");
        Console.WriteLine($"Number Of Lines: {AllLines.Count}");
        Console.WriteLine($"Number Of Trains: {AllTransits.Count}");
    }
}
