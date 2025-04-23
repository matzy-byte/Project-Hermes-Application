using Godot;
using System;

public partial class TrainScript : StaticBody3D
{
    public int TrainID;
    public bool Driving;
    public bool InStation;
    public bool DrivingForward;
    public StationScript CurrentStation;
    public StationScript NextStation;
    public float TravelDistance;
    public float WaitingTime;

    public void Setup(int trainID, bool driving, bool inStation, bool drivingForward, StationScript currentStation, StationScript nextStation, float travelDistance, float waitingTime)
    {
        TrainID = trainID;
        Driving = driving;
        InStation = inStation;
        DrivingForward = drivingForward;
        CurrentStation = currentStation;
        NextStation = nextStation;
        TravelDistance = travelDistance;
        WaitingTime = waitingTime;
    }

    public void Update(float travelDistance, float waitingTime)
    {
        TravelDistance = travelDistance;
        WaitingTime = waitingTime;
    }

    public void UpdateRoute(bool driving, bool inStation, bool drivingForward, StationScript currentStation, StationScript nextStation, float travelDistance, float waitingTime)
    {
        Driving = driving;
        InStation = inStation;
        DrivingForward = drivingForward;
        CurrentStation = currentStation;
        NextStation = nextStation;
        TravelDistance = travelDistance;
        WaitingTime = waitingTime;
    }

    public override void _PhysicsProcess(double delta)
    {
        Position = new Vector3(
            Mathf.Lerp(CurrentStation.Position.X, NextStation.Position.X, TravelDistance),
            0,
            Mathf.Lerp(CurrentStation.Position.Z, NextStation.Position.Z, TravelDistance)
            );
        LookAt(NextStation.Position, Vector3.Up);
    }

}
