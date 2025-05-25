using Godot;
using shared;
using Singletons;
using Stations;
using System;

namespace Trains;

public partial class TrainScript : StaticBody3D
{
    public TrainData Data { get; set; }
    private StationScript currentStation;
    private StationScript nextStation;

    public override void _PhysicsProcess(double delta)
    {
        Position = new Vector3(
        Mathf.Lerp(currentStation.GlobalPosition.X, nextStation.GlobalPosition.X, Data.TravelDistance),
        0,
        Mathf.Lerp(currentStation.GlobalPosition.Z, nextStation.GlobalPosition.Z, Data.TravelDistance)
        );

        if (GlobalPosition != nextStation.GlobalPosition)
        {
            LookAt(nextStation.GlobalPosition, Vector3.Up);
        }
    }

    public void Initialize(TrainData data)
    {
        Data = data;
        currentStation = GameManagerScript.Instance.Stations.Find(station => station.Data.StationId == Data.CurrentStationId);
        nextStation = GameManagerScript.Instance.Stations.Find(station => station.Data.StationId == Data.NextStationId);
    }

    public void Update(TrainData data)
    {
        Data = data;
        if (currentStation.Data.StationId != Data.CurrentStationId || nextStation.Data.StationId != Data.NextStationId)
        {
            currentStation = GameManagerScript.Instance.Stations.Find(station => station.Data.StationId == Data.CurrentStationId);
            nextStation = GameManagerScript.Instance.Stations.Find(station => station.Data.StationId == Data.NextStationId);
        }
    }
}
