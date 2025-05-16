using Newtonsoft.Json;

namespace Z;

public static class DataManager
{
    public static List<Transit> AllTransits { get; set; } = [];
    public static List<Line> AllLines { get; set; } = [];

    //Paths to the json files
    private const string PATHSTATIONJSON = "shared\\jsondata\\haltestellen_v2.json";
    private const string PATHLINEJSON = "shared\\jsondata\\lines_v2.json";
    private const string PATHGEODATA = "shared\\jsondata\\KVVLinesGeoJSON.json";
    private const string PATHTRANSITINFO = "shared\\jsondata\\KVV_Transit_Information.json";

    private static List<Station> allStations = [];
    private static List<GeoData> allGeoDataPoints = [];

    public static void LoadDataFromJson()
    {
        allStations = JsonReader.LoadListedData<Station, StationWrapper>(PATHSTATIONJSON, x => new Station(x));
        allGeoDataPoints = JsonReader.LoadNestedListData<GeoData, FeatureCollectionWrapper, FeatureWrapper>(PATHGEODATA, x => x.Features, x => new GeoData(x.Properties, x.Geometry));
        AllLines = JsonReader.LoadNestedListData<Line, RootObjectWrapper, LineWrapper>(PATHLINEJSON, x => x.Lines, x => new Line(x, FindGeoPointsByLine(x.Number)));
        AllTransits = JsonReader.LoadListedData<Transit, TransitInfoWrapper>(PATHTRANSITINFO, x => new Transit(x));

        RearangeLines();
        //Console.WriteLine($"{allStations.Count}, {allLines.Count}, {AllTransits.Count}");
        //Console.WriteLine(JsonConvert.SerializeObject(allLines[0]));
    }

    public static Line FindLineByLineName(string lineName)
    {
        Line line = AllLines.Find(x => x.LineName == lineName);
        return line;
    }

    private static List<Coordinate> FindGeoPointsByLine(string lineName)
    {
        GeoData data = allGeoDataPoints.Find(x => x.Name == lineName);
        if (data == null || data.Coordinates == null)
            return [];
        return data.Coordinates;
    }

    private static void RearangeLines()
    {
        foreach (Transit transit in AllTransits)
        {
            int startStationIndex = transit.Line.Stations.IndexOf(transit.StartStationId);
            int endStationIndex = transit.Line.Stations.IndexOf(transit.DestinationStationId);

            //When startindex is bigger than end index
            if (startStationIndex > endStationIndex)
            {
                transit.Line.Stations.Reverse();
                transit.Line.Coordinates.Reverse();
                Console.WriteLine("Rearanged Data for Line: " + transit.LineName);
            }
        }
    }
}