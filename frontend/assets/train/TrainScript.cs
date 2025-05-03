using Godot;
using System;

public partial class TrainScript : StaticBody3D
{
    public int TrainID;
    public StationScript CurrentStation;
    public StationScript NextStation;
    public float TravelDistance;
    public float WaitingTime;

    public void Setup(int trainID, StationScript currentStation, StationScript nextStation, float travelDistance, float waitingTime)
    {
        TrainID = trainID;
        GetNode<Label>("%Label").Text = trainID.ToString();
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

    public void UpdateRoute(StationScript currentStation, StationScript nextStation, float travelDistance, float waitingTime)
    {
        CurrentStation = currentStation;
        NextStation = nextStation;
        TravelDistance = travelDistance;
        WaitingTime = waitingTime;
    }

    public override void _PhysicsProcess(double delta)
    {
        Position = new Vector3(
        Mathf.Lerp(CurrentStation.GlobalPosition.X, NextStation.GlobalPosition.X, TravelDistance),
        0,
        Mathf.Lerp(CurrentStation.GlobalPosition.Z, NextStation.GlobalPosition.Z, TravelDistance)
        );
        if (GlobalPosition != NextStation.GlobalPosition)
            LookAt(NextStation.GlobalPosition, Vector3.Up);
    }
}
