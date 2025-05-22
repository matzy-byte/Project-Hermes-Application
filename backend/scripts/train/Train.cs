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
        }
        //Flip driving direction if station is at Start
        else if (DrivingForward == false && CurrentStationId == StationIds.First())
        {
            DrivingForward = true;
        }

        NextStationId = FindNextStation();
        timeBetweenStations = GetTimeBetweenStations();
        inStationTimer = 0;
    }

    public void ExitStation()
    {
        InStation = false;
        drivingTimer = 0;
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

    public float NextPickupTime(string stationId, bool drivingForward, float time)
    {
        var timeTable = drivingForward ? TrainManager.TimeTableForward : TrainManager.TimeTableBackwards;
        foreach (float pickupTime in timeTable[TrainId][stationId])
        {
            if (pickupTime >= time)
            {
                return pickupTime;
            }
        }
        return float.PositiveInfinity;
    }

    public float GetTravelTime(string enterStationId, string exitStationId, bool drivingForward)
    {
        float exitTime;
        float enterTime;
        if (drivingForward)
        {
            //Edge cases where exit station is last station
            if (exitStationId == StationIds.Last())
            {
                exitTime = TrainManager.TimeTableBackwards[TrainId][exitStationId].First();
            }
            else
            {
                exitTime = TrainManager.TimeTableForward[TrainId][exitStationId].First();
            }
            enterTime = TrainManager.TimeTableForward[TrainId][enterStationId].First();
        }
        else
        {
            //edge case where exit station is first station
            if (exitStationId == StationIds.First())
            {
                exitTime = TrainManager.TimeTableForward[TrainId][exitStationId][1];
            }
            else
            {
                exitTime = TrainManager.TimeTableBackwards[TrainId][exitStationId].First();
            }
            enterTime = TrainManager.TimeTableBackwards[TrainId][enterStationId].First();
        }
        return exitTime - enterTime - SimulationSettings.SimulationSettingsParameters.TrainWaitingTimeAtStation;

    }
}