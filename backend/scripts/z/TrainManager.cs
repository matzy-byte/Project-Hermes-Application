using Newtonsoft.Json;

namespace Z;

public static class TrainManager
{
    public static List<Train> AllTrains { get; set; } = [];
    public static List<string> AllStations { get; set; } = [];

    public static Dictionary<int, Dictionary<string, List<float>>> TimeTableForward = [];
    public static Dictionary<int, Dictionary<string, List<float>>> TimeTableBackwards = [];

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
        GenerateTimeTables();
        Console.WriteLine($"Number Of Trains Initialized: {AllTrains.Count}");
    }

    public static void UpdateAllTrains()
    {
        foreach (Train train in AllTrains)
        {
            train.TrainUpdate();
        }
    }

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

    private static void LoadAllUsedStations()
    {
        AllStations = [.. AllTrains.SelectMany(train => train.StationIds).Distinct()];
    }

    private static void GenerateTimeTables()
    {
        // Clear and initialize dictionaries
        TimeTableForward.Clear();
        TimeTableBackwards.Clear();

        foreach (Train train in AllTrains)
        {
            int trainId = train.TrainId;
            List<string> stations = train.StationIds;
            string startStation = stations.First();
            string endStation = stations.Last();
            bool goingForward = true;

            TimeTableForward[trainId] = stations.ToDictionary(s => s, _ => new List<float>());
            TimeTableBackwards[trainId] = stations.ToDictionary(s => s, _ => new List<float>());

            for (int i = 0; i < SimulationSettingsGlobal.PreComputedStopTimes * 2; i++)
            {
                float timeBetweenStations = (goingForward ? train.TravelTime : train.TravelTimeReverse) * 60 / stations.Count;

                for (int j = 0; j < stations.Count; j++)
                {
                    //Get the correct station
                    string station = goingForward ? stations[j] : stations[stations.Count - 1 - j];
                    var timeTable = goingForward ? TimeTableForward : TimeTableBackwards;

                    bool isTerminal = goingForward ? station == endStation : station == startStation;
                    if (isTerminal)
                    {
                        continue;
                    }

                    float exitTime;

                    if (i == 0 && j == 0 && goingForward)
                    {
                        exitTime = SimulationSettings.SimulationSettingsParameters.TrainWaitingTimeAtStation;
                    }
                    else
                    {
                        float previousExit;

                        if (j == 0)
                        {
                            // Coming from other direction
                            string otherStation = goingForward ? stations[1] : stations[stations.Count - 2];
                            previousExit = (goingForward ? TimeTableBackwards : TimeTableForward)[trainId][otherStation].Last();
                        }
                        else
                        {
                            int previousIndex = goingForward ? j - 1 : j - 1;
                            string previousStation = goingForward
                                ? stations[previousIndex]
                                : stations[stations.Count - 1 - previousIndex];

                            previousExit = timeTable[trainId][previousStation].Last();
                        }

                        float entryTime = previousExit + timeBetweenStations;
                        exitTime = entryTime + SimulationSettings.SimulationSettingsParameters.TrainWaitingTimeAtStation;
                    }
                    timeTable[trainId][station].Add(exitTime);
                }
                // Switch direction
                goingForward = !goingForward;
            }
        }
    }
}