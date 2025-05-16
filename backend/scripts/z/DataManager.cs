namespace Z;

public static class DataManager
{
    public static List<TransitInfoWrapper> AllTransits { get; set; } = [];
    public static List<LineWrapper> AllLines { get; set; } = [];

    //Paths to the json files
    private const string PATHSTATIONJSON = "shared\\jsondata\\haltestellen_v2.json";
    private const string PATHLINEJSON = "shared\\jsondata\\lines_v2.json";
    private const string PATHGEODATA = "shared\\jsondata\\KVVLinesGeoJSON.json";
    private const string PATHTRANSITINFO = "shared\\jsondata\\KVV_Transit_Information.json";

    private static List<Station> allStations = [];

    public static void LoadDataFromJson()
    {
        allStations = JsonReader.LoadListedData<Station, StationWrapper>(PATHSTATIONJSON, x => new Station(x));
        AllLines = JsonReader.LoadNestedListData<RootObjectWrapper, LineWrapper>(PATHLINEJSON, x => x.Lines);
        AllTransits = JsonReader.LoadData<List<TransitInfoWrapper>>(PATHTRANSITINFO);

        //RearangeLines();
        //Console.WriteLine($"{allStations.Count}, {allLines.Count}, {AllTransits.Count}");
        //Console.WriteLine(JsonConvert.SerializeObject(allLines[0]));
    }
}