using Godot;
using shared;

namespace Singletons;

public partial class GameManagerScript : Node
{
    public static GameManagerScript Instance { get; set; }
    public SimulationSettingsData SimulationSettings { get; set; }

    private PackedScene stationScene = ResourceLoader.Load<PackedScene>("res://assets/station/Station.tscn");
    private PackedScene trainScene = ResourceLoader.Load<PackedScene>("res://assets/train/Train.tscn");
    private PackedScene robotScene = ResourceLoader.Load<PackedScene>("res://assets/robot/Robot.tscn");

    public override void _Ready()
    {
        Instance = this;
    }

    public void StartSimulation()
    {
    }

    public void StopSimulation()
    {
    }

    public void PauseSimulation(bool isPaused)
    {
    }

    public void SpawnStations(StationData[] stations)
    {
    }

    public void SpawnTrains(TrainData[] trains)
    {
    }

    public void SpawnRobots(RobotData[] robots)
    {
    }

    public void UpdateTrains(TrainData[] trains)
    {
    }

    public void UpdateRobots(RobotData[] robots)
    {
    }

}
