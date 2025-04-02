using System.Security.Cryptography.X509Certificates;
using json;
public static class LineManager
{
    //Paths to the json files
    const string PATHSTATIONJSON = "jsondata\\haltestellen_v2.json";
    const string PATHLINEJSON = "jsondata\\lines_v2.json";
    const string PATHGEODATA = "jsondata\\KVVLinesGeoJSON.json";
    const string TRANSITINFOPATH = "jsondata\\KVV_Transit_Information.json";
    public static Line[] lines = null;

    /// <summary>
    /// loads all data from the json files regarding the train lines
    /// </summary>
    public static void initialize()
    {
        Console.WriteLine("Start Initializing Line Data from JSON Files ...");
        Station[] stations;
        GeoData[] geoDatas;
        TransitInfo[] transitInfos;
        //Load all the data from the json files
        try
        {
            stations = JSONReader.loadStations(PATHSTATIONJSON);
            lines = JSONReader.loadLines(PATHLINEJSON, stations);
            geoDatas = JSONReader.loadGeoData(PATHGEODATA);
            transitInfos = JSONReader.loadTransitInfo(TRANSITINFOPATH);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw ex;
        }

        //Add geodata and transitinfos to the lines
        addGeoDataToLines(geoDatas);
        addTransitInfoToLines(transitInfos);

        Console.WriteLine("Initialized Line Data from JSON files");
    }

    /// <summary>
    /// Adds the GeoData to the Lines
    /// </summary>
    private static void addGeoDataToLines(GeoData[] geoDatas)
    {
        //Loops over each geoData object and adds it to the line that has the same name or number
        for (int i = 0; i < geoDatas.Length; i++)
        {
            string lineName = geoDatas[i].name;

            for (int j = 0; j < lines.Length; j++)
            {
                if (lines[j].disassembledName == lineName || lines[j].number == lineName)
                {
                    lines[j].geoData = geoDatas[i];
                    break;
                }
            }
        }
    }


    /// <summary>
    /// Adds the TransitInfos to the Lines
    /// </summary>
    private static void addTransitInfoToLines(TransitInfo[] transitInfos)
    {
        //Loops over each transitInfo object and adds it to the line that has the same name
        for (int i = 0; i < transitInfos.Length; i++)
        {
            string lineName = transitInfos[i].lineName;

            for (int j = 0; j < lines.Length; j++)
            {
                if (lines[j].name == lineName)
                {
                    lines[j].transitInfo = transitInfos[i];
                    break;
                }
            }
        }
    }
}