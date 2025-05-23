using Godot;
using shared;
using Singletons;
using Stations;
using Trains;

namespace Robots;

public partial class RobotScript : StaticBody3D
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
        currentTrain = GameManagerScript.Instance.Trains.Find(train => train.Data.TrainId == Data.TrainId);
        if (currentStation != null)
        {
            currentStation.AddChild(this);
            GlobalPosition = currentStation.GlobalPosition;
            return;
        }
        if (currentTrain != null)
        {
            currentTrain.AddChild(this);
            GlobalPosition = currentTrain.GlobalPosition;
        }
    }

    public void Update(RobotData data)
    {
        Data = data;
        if (currentStation.Data.StationId != Data.CurrentStationId || currentTrain.Data.TrainId != Data.TrainId)
        {
            currentStation = GameManagerScript.Instance.Stations.Find(station => station.Data.StationId == Data.CurrentStationId);
            currentTrain = GameManagerScript.Instance.Trains.Find(train => train.Data.TrainId == Data.TrainId);
            if (currentStation != null)
            {
                GetParent().RemoveChild(this);
                currentStation.AddChild(this);
                GlobalPosition = currentStation.GlobalPosition;
                return;
            }
            if (currentTrain != null)
            {
                GetParent().RemoveChild(this);
                currentTrain.AddChild(this);
                GlobalPosition = currentTrain.GlobalPosition;
            }
        }
    }
}
