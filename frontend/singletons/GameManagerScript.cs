using System.Collections.Generic;
using Godot;
using Newtonsoft.Json;
using Robots;
using shared;
using Stations;
using Trains;
using UI;

namespace Singletons;

public partial class GameManagerScript : Node
{
    public static GameManagerScript Instance { get; set; }
    public SimulationSettingsData SimulationSettings { get; set; }
    public List<StationScript> Stations { get; set; } = [];
    public List<TrainScript> Trains { get; set; } = [];
    public List<RobotScript> Robots { get; set; } = [];

    private PackedScene stationScene = ResourceLoader.Load<PackedScene>("res://assets/station/Station.tscn");
    private PackedScene trainScene = ResourceLoader.Load<PackedScene>("res://assets/train/Train.tscn");
    private PackedScene robotScene = ResourceLoader.Load<PackedScene>("res://assets/robot/Robot.tscn");

    public override void _Ready()
    {
        Instance = this;
    }

    public static void SetSimulationConfiguration(SimulationSettingsData simulationSettingsData)
    {
        WebSocketMessage settingsMessage = new(203, MessageType.SETTINGS, JsonConvert.SerializeObject(simulationSettingsData));
        SessionManager.Instance.Request(settingsMessage);
    }

    public static void StartSimulation()
    {
        SessionManager.Instance.Request(102, MessageType.STARTSIMULATION);
    }

    public static void StopSimulation()
    {
        SessionManager.Instance.Request(102, MessageType.STOPSIMULATION);
    }

    public static void PauseSimulation(bool isPaused)
    {
        if (isPaused)
        {
            SessionManager.Instance.Request(102, MessageType.CONTINUESTIMULATION);
            return;
        }
        SessionManager.Instance.Request(102, MessageType.PAUSESIMULATION);
    }

    public void SpawnStations(List<StationData> stations)
    {
        foreach (StationData stationData in stations)
        {
            StationScript station = stationScene.Instantiate<StationScript>();
            GetTree().CurrentScene.AddChild(station);
            station.Initialize(stationData);
            Stations.Add(station);
        }

        GetTree().CurrentScene.GetNode<LoadingStationsMenuScript>("HUD/LoadingStationsMenu").UpdateStationsOption(stations);
        GetTree().CurrentScene.GetNode<ChargingStationsMenuScript>("HUD/ChargingStationsMenu").UpdateStationsOption(stations);
    }

    public void SpawnTrains(List<TrainData> trains)
    {
        foreach (TrainData trainData in trains)
        {
            TrainScript train = trainScene.Instantiate<TrainScript>();
            GetTree().CurrentScene.AddChild(train);
            train.Initialize(trainData);
            Trains.Add(train);
        }
    }

    public void SpawnRobots(List<RobotData> robots)
    {
        foreach (RobotData robotData in robots)
        {
            RobotScript robot = robotScene.Instantiate<RobotScript>();
            robot.Initialize(robotData);
            Robots.Add(robot);
        }
    }

    public void UpdateTrains(List<TrainData> trains)
    {
        foreach (TrainData trainData in trains)
        {
            TrainScript train = this.Trains.Find(train => train.Data.TrainId == trainData.TrainId);
            train?.Update(trainData);
        }
    }

    public void UpdateRobots(List<RobotData> robots)
    {
        foreach (RobotData robotData in robots)
        {
            RobotScript robot = this.Robots.Find(train => train.Data.TrainId == robotData.RobotId);
            robot?.Update(robotData);
        }
    }

    public void Reset()
    {
        Robots.ForEach(robot => robot.QueueFree());
        Robots.Clear();
        Trains.ForEach(train => train.QueueFree());
        Trains.Clear();
        Stations.ForEach(station => station.QueueFree());
        Stations.Clear();
    }
}
