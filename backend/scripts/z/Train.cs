using shared;
using Simulation;

namespace Z;

public class Train : TrainData
{
    public Transit Transit { get; set; }

    //Timer to manage timings of driving and standing
    private float inStationTimer = 0f;
    private float drivingTimer = 0f;
    private float timeBetweenStations;

    public Train(Transit transit, int id)
    {
        TrainId = id;
        Transit = transit;
        InitializeTrain();
    }

    public void InitializeTrain()
    {
        InStation = true;
        DrivingForward = true;
        TravelDistance = 0f;
        CurrentStationId = Transit.StartStationId;
        NextStationId = FindNextStation();
        timeBetweenStations = GetTimeBetweenStations();
    }

    public void TrainUpdate()
    {
        if (InStation == false)
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
            if (CurrentStationId == Transit.DestinationStationId)
                throw new Exception("Cant find station --> Train is at end station with wrong driving direction TrainLine:" + Transit.LineName);

            return Transit.Line.Stations[Transit.Line.Stations.IndexOf(CurrentStationId) + 1];
        }

        if (CurrentStationId == Transit.StartStationId)
            throw new Exception("Cant find station --> Train is at end station with wrong driving direction TrainLine:" + Transit.LineName);

        return Transit.Line.Stations[Transit.Line.Stations.IndexOf(CurrentStationId) - 1];
    }

    private float GetTimeBetweenStations()
    {
        float totalTime = Transit.TravelTime;
        if (!DrivingForward)
            totalTime = Transit.TravelTimeReverse;

        //Get's travel time between stations in seconds (all stations have the same travel time)
        return totalTime * 60 / Transit.Line.Stations.Count;
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
        WaitingTime = inStationTimer / SimulationSettings.trainWaitingTimeAtStation;

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
        if (DrivingForward && CurrentStationId == Transit.DestinationStationId)
        {
            DrivingForward = false;
        }
        //Flip driving direction if station is at Start
        else if (DrivingForward == false && CurrentStationId == Transit.StartStationId)
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
        
}