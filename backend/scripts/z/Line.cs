using Newtonsoft.Json;
using shared;

namespace Z;

public class Line : LineData
{
    public Line(LineWrapper lineWrapper, List<Coordinate> coordinates)
    {
        LineName = lineWrapper.Name;
        Stations = lineWrapper.Stations;
        Coordinates = [.. coordinates.Cast<CoordinateData>()];

        RemoveLoopingStations();
    }

    private void RemoveLoopingStations()
    {
        List<string> singleInstanceStationIds = [];

        if (Stations[0] != Stations.Last())
            return;

        foreach (string id in Stations)
        {
            if (singleInstanceStationIds.Contains(id) == false)
            {
                singleInstanceStationIds.Add(id);
                continue;
            }
            Console.WriteLine("Looping Stations found at: " + LineName);
            break;
        }

        Stations = singleInstanceStationIds;
        Console.WriteLine(JsonConvert.SerializeObject(Stations));
    }
}