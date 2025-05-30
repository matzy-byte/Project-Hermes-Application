using System.Reflection.Metadata.Ecma335;
using shared;
using Simulation;


namespace Trains;

public class Train : TrainData
{
    public string LineName { get; set; }
    public List<string> StationIds { get; set; }
    public float TravelTime { get; set; }
    public float TravelTimeReverse { get; set; }

    //Timer to manage timings of driving and standing
    private float inStationTimer = 0f;
    private float drivingTimer = 0f;
    private float timeBetweenStations;

    public Train(string lineName, List<string> stationids, float travelTime, float travelTimeReverse, int id)
    {
        TrainId = id;
        LineName = lineName;
        StationIds = stationids;
        TravelTime = travelTime;
        TravelTimeReverse = travelTimeReverse;
        InitializeTrain();
    }

    public void InitializeTrain()
    {
        InStation = true;
        DrivingForward = true;
        TravelDistance = 0f;
        CurrentStationId = StationIds.First();
        NextStationId = FindNextStation();
        timeBetweenStations = GetTimeBetweenStations();
    }

    public void TrainUpdate()
    {
        if (!InStation)
        {
            UpdateDriving();
            return;
        }
        UpdateInStation();
    }

    private string FindNextStation()
    {
        if (DrivingForward)
        {
            if (CurrentStationId == StationIds.Last())
            {
                throw new Exception("Cant find station --> Train is at end station with wrong driving direction TrainLine:" + LineName);
            }

            return StationIds[StationIds.IndexOf(CurrentStationId) + 1];
        }

        if (CurrentStationId == StationIds.First())
        {
            throw new Exception("Cant find station --> Train is at end station with wrong driving direction TrainLine:" + LineName);
        }

        return StationIds[StationIds.IndexOf(CurrentStationId) - 1];
    }

    private float GetTimeBetweenStations()
    {
        float totalTime = TravelTime;
        if (!DrivingForward)
        {
            totalTime = TravelTimeReverse;
        }

        //Get's travel time between stations in seconds (all stations have the same travel time)
        return totalTime * 60 / StationIds.Count;
    }

    private void UpdateDriving()
    {
        drivingTimer += SimulationManager.scaledDeltaTime;
        TravelDistance = drivingTimer / timeBetweenStations;

        //Update when station is reached
        if (TravelDistance >= 1)
            EnterStation();
    }

    private void UpdateInStation()
    {
        inStationTimer += SimulationManager.scaledDeltaTime;
        WaitingTime = inStationTimer / SimulationSettings.SimulationSettingsParameters.TrainWaitingTimeAtStation;

        // Update when time is over
        if (WaitingTime >= 1)
            ExitStation();
    }

    public void EnterStation()
    {
        InStation = true;
        TravelDistance = 0f;
        CurrentStationId = NextStationId;

        //Flip driving direction if station is at end
        if (DrivingForward && CurrentStationId == StationIds.Last())
        {
            DrivingForward = false;
            DataLogger.AddLog("Train " + TrainId + " Changed Driving Direction to DrivingForward: " + DrivingForward.ToString());
        }
        //Flip driving direction if station is at Start
        else if (DrivingForward == false && CurrentStationId == StationIds.First())
        {
            DrivingForward = true;
            DataLogger.AddLog("Train " + TrainId + " Changed Driving Direction to DrivingForward: " + DrivingForward.ToString());
        }

        NextStationId = FindNextStation();
        timeBetweenStations = GetTimeBetweenStations();
        inStationTimer = 0;

        DataLogger.AddLog("Train " + TrainId + " Enterd Station " + CurrentStationId);

    }

    public void ExitStation()
    {
        InStation = false;
        drivingTimer = 0;

        DataLogger.AddLog("Train " + TrainId + " Exited Station " + CurrentStationId + " Next Station " + NextStationId);
    }

    public List<string> GetBetweenStation(string startStationId, string destinationId)
    {
        int enterStationIndex = StationIds.IndexOf(startStationId);
        int exitStationIndex = StationIds.IndexOf(destinationId);
        bool drivingForward = enterStationIndex < exitStationIndex;

        List<string> stations = [];
        if (drivingForward)
        {
            for (int i = enterStationIndex; i < exitStationIndex + 1; i++)
            {
                stations.Add(StationIds[i]);
            }
            return stations;
        }

        for (int i = enterStationIndex; i >= exitStationIndex; i--)
        {
            stations.Add(StationIds[i]);
        }
        return stations;
    }

    public float GetTravelTime(string enterStationId, string exitStationId, bool drivingForward)
    {
        float enterTime = NextPickupTime(enterStationId, drivingForward, 0);
        float exitTime;

        if (drivingForward)
        {
            bool isLastStation = exitStationId == StationIds.Last();
            exitTime = NextPickupTime(exitStationId, !isLastStation ? true : false, 0);
        }
        else
        {
            bool isFirstStation = exitStationId == StationIds.First();
            exitTime = NextPickupTime(exitStationId, isFirstStation ? true : false, 0);

            if (isFirstStation)
            {
                exitTime = NextPickupTime(
                    exitStationId,
                    true,
                    exitTime + SimulationSettings.SimulationSettingsParameters.TrainWaitingTimeAtStation * 2
                );
            }
        }

        return exitTime - enterTime - SimulationSettings.SimulationSettingsParameters.TrainWaitingTimeAtStation;
    }

    public float NextPickupTime(string stationId, bool drivingForward, float time)
    {
        //Create Dictionary
        Dictionary<string, List<float>> timeTableForward = StationIds.ToDictionary(s => s, _ => new List<float>());
        Dictionary<string, List<float>> timeTableBackward = StationIds.ToDictionary(s => s, _ => new List<float>());

        float pickupTime = 0;
        bool pickupIsTerminal = drivingForward ? stationId == StationIds.Last() : stationId == StationIds.First();
        bool endLoop = false;
        bool goingForward = true;
        int iterations = 0;
        int maxIterations = 1000;

        do
        {
            float timeBetweenStations = (goingForward ? TravelTime : TravelTimeReverse) * 60 / StationIds.Count;

            for (int j = 0; j < StationIds.Count; j++)
            {
                //Get the correct station
                string station = goingForward ? StationIds[j] : StationIds[StationIds.Count - 1 - j];
                var timeTable = goingForward ? timeTableForward : timeTableBackward;

                bool isTerminal = goingForward ? station == StationIds.Last() : station == StationIds.First();
                if (isTerminal)
                {
                    continue;
                }

                float exitTime;

                //Very first entry is hardcoded
                if (iterations == 0)
                {
                    exitTime = SimulationSettings.SimulationSettingsParameters.TrainWaitingTimeAtStation;
                }
                else
                {
                    float previousExit;

                    if (j == 0)
                    {
                        // Coming from other direction
                        string otherStation = goingForward ? StationIds[1] : StationIds[StationIds.Count - 2];
                        previousExit = (goingForward ? timeTableBackward : timeTableForward)[otherStation].Last();
                    }
                    else
                    {
                        int previousIndex = goingForward ? j - 1 : j - 1;
                        string previousStation = goingForward
                            ? StationIds[previousIndex]
                            : StationIds[StationIds.Count - 1 - previousIndex];

                        previousExit = timeTable[previousStation].Last();
                    }

                    float entryTime = previousExit + timeBetweenStations;
                    exitTime = entryTime + SimulationSettings.SimulationSettingsParameters.TrainWaitingTimeAtStation;
                }
                timeTable[station].Add(exitTime);
            }

            //Check the number of Values for Forward and backward are over 1 (both have at least one )
            if (timeTableBackward.Sum(kvp => kvp.Value.Count) > 0 && timeTableForward.Sum(kvp => kvp.Value.Count) > 0)
            {
                //Get the time and check if loop can be exited
                pickupTime = drivingForward ? timeTableForward[stationId].Last() : timeTableBackward[stationId].Last();
                endLoop = pickupTime >= time;

                //Return just a big value
                if (iterations >= maxIterations)
                {
                    return float.PositiveInfinity / 2f;
                }
            }

            // Switch direction
            goingForward = !goingForward;
            iterations++;
        }
        while (!endLoop);

        return pickupTime;
    }
}