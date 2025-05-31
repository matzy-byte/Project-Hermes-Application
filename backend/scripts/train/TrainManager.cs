using Helper;
using Json;
using Simulation;

namespace Trains;

public static class TrainManager
{
    public static List<Train> AllTrains { get; set; } = [];
    public static List<string> AllStations { get; set; } = [];

    /// <summary>
    /// Initializes all Trains
    /// </summary>
    public static void Initialize()
    {
        AllTrains = [];
        int trainIndex = 0;
        foreach (TransitInfoWrapper transit in DataManager.AllTransits)
        {
            AllTrains.Add(new Train(transit.LineName, GetStationIds(transit), transit.TravelTime, transit.TravelTimeReverse, trainIndex));
            trainIndex++;
        }
        LoadAllUsedStations();
        DataLogger.AddLog("Time Table for Trains Initialized: " + AllTrains.Count);

        Console.WriteLine($"Number Of Trains Initialized: {AllTrains.Count}");
        DataLogger.AddLog("Number Of Trains Initialized: " + AllTrains.Count);

    }

    /// <summary>
    /// Updates all Trains
    /// </summary>
    public static void UpdateAllTrains()
    {
        foreach (Train train in AllTrains)
        {
            train.TrainUpdate();
        }
    }

    /// <summary>
    /// Gets the stationIds from TransitInfo wrapper in the correct order
    /// </summary>
    private static List<string> GetStationIds(TransitInfoWrapper transit)
    {
        LineWrapper line = DataManager.AllLines.Find(x => x.Name == transit.LineName);

        // Remove looping stations
        if (line.Stations.First() == line.Stations.Last())
        {
            line.Stations = [.. line.Stations.Distinct()];
        }

        int startStationIndex = line.Stations.IndexOf(transit.StartStationID);
        int destinationStationIndex = line.Stations.IndexOf(transit.DestinationID);

        bool forward = startStationIndex < destinationStationIndex;
        List<string> stationIds = [];
        if (!forward)
        {
            line.Stations.Reverse();
            startStationIndex = line.Stations.IndexOf(transit.StartStationID);
            destinationStationIndex = line.Stations.IndexOf(transit.DestinationID);
        }

        for (int i = startStationIndex; i < destinationStationIndex + 1; i++)
        {
            stationIds.Add(line.Stations[i]);
        }
        return stationIds;
    }

    /// <summary>
    /// Loads all stations that are used in the Simulation into the AllStations variable
    /// </summary>
    private static void LoadAllUsedStations()
    {
        AllStations = [.. AllTrains.SelectMany(train => train.StationIds).Distinct()];
    }

}