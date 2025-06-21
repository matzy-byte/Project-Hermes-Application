using System.Collections.Generic;
using System.Linq;
using CommandLine;
using Godot;
using Interface;
using Robots;
using shared;
using Singletons;
using Stations;
using UI;

namespace Trains;

public partial class TrainScript : StaticBody3D, IInteractable
{
    public TrainData Data { get; set; }
    private string lineName;
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

    public void Initialize(TrainData data, string lineName)
    {
        Data = data;
        this.lineName = lineName;
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

    public Node3D Select()
    {
        ((HUDScript)GetTree().GetFirstNodeInGroup("HUD")).ObjectInfo.ShowInfo(GetInfo(), this);
        return this;
    }

    public Dictionary<string, string> GetInfo()
    {
        string robots = string.Join(", ", GetChildren().OfType<RobotScript>().Select(r => r.Data.RobotId.ToString()));
        string nextStationName = GameManagerScript.Instance.Stations.Find(station => station.Data.StationId == Data.NextStationId).Data.StationName;
        return new Dictionary<string, string>
        {
            ["Train ID"] = Data.TrainId.ToString(),
            ["Line"] = lineName,
            ["Next Station"] = nextStationName,
            ["Travel Progress"] = Data.TravelDistance.ToString(),
            ["Robots"] = robots
        };
    }
}
