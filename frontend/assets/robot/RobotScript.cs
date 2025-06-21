using System.Collections.Generic;
using System.Linq;
using Godot;
using Interface;
using shared;
using Singletons;
using Stations;
using Trains;
using UI;

namespace Robots;

public partial class RobotScript : StaticBody3D, IInteractable
{
    public RobotData Data { get; set; }
    private StationScript currentStation;
    private TrainScript currentTrain;

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    }

    public void Initialize(RobotData data)
    {
        Data = data;
        currentStation = GameManagerScript.Instance.Stations.Find(station => station.Data.StationId == Data.CurrentStationId);
        currentStation.AddChild(this);
        GlobalPosition = currentStation.GlobalPosition;
    }

    public void Update(RobotData data)
    {
        if (Data.OnStation == data.OnStation && Data.OnTrain == data.OnTrain)
        {
            Data = data;
            return;
        }

        if (data.TrainId == -1)
        {
            currentTrain = null;
            currentStation = GameManagerScript.Instance.Stations.Find(station => station.Data.StationId == data.CurrentStationId);
            GetParent().RemoveChild(this);
            currentStation.AddChild(this);
            GlobalPosition = currentStation.GlobalPosition;
        }
        else
        {
            currentTrain = GameManagerScript.Instance.Trains.Find(train => train.Data.TrainId == data.TrainId);
            GetParent().RemoveChild(this);
            currentTrain.AddChild(this);
            GlobalPosition = currentTrain.GlobalPosition;
        }

        if (currentStation.Data.StationName != data.CurrentStationId)
        {
            currentStation = GameManagerScript.Instance.Stations.Find(station => station.Data.StationId == data.CurrentStationId);
        }
        Data = data;
    }

    public Node3D Select()
    {
        ((HUDScript)GetTree().GetFirstNodeInGroup("HUD")).ObjectInfo.ShowInfo(GetInfo(), this);
        return this;
    }

    public Dictionary<string, string> GetInfo()
    {
        int loadedPackages = 0;
        foreach (List<PackageData> packages in Data.LoadedPackages.Values)
        {
            loadedPackages += packages.Count;
        }
        string destinationStationName = "";
        if (Data.TotalPath.Count >= 1)
        {
            destinationStationName = GameManagerScript.Instance.Stations.Find(station => station.Data.StationId == Data.TotalPath.Last().StationIds.Last()).Data.StationName;
        }
        return new Dictionary<string, string>
        {
            ["Robot ID"] = Data.RobotId.ToString(),
            ["Battery Capacity"] = Data.BatteryCapacity.ToString(),
            ["Loaded Packages"] = loadedPackages.ToString(),
            ["Destination Station"] = destinationStationName,
            ["Current Train"] = currentTrain == null ? "None" : Data.TrainId.ToString(),
            ["Current / Latest Station"] = currentStation == null ? "None" : currentStation.Data.StationName
        };
    }

}
