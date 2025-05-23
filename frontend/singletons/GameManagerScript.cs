using System.Collections.Generic;
using Godot;
using Robots;
using shared;
using Stations;
using Trains;

namespace Singletons;

public partial class GameManagerScript : Node
{
    public static GameManagerScript Instance { get; set; }
    public SimulationSettingsData SimulationSettings { get; set; }

    private PackedScene stationScene = ResourceLoader.Load<PackedScene>("res://assets/station/Station.tscn");
    private PackedScene trainScene = ResourceLoader.Load<PackedScene>("res://assets/train/Train.tscn");
    private PackedScene robotScene = ResourceLoader.Load<PackedScene>("res://assets/robot/Robot.tscn");
    private List<StationScript> stations = [];
    private List<TrainScript> trains = [];
    private List<RobotScript> robots = [];

    public override void _Ready()
    {
        Instance = this;
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
            station.Initialize(stationData);
            this.stations.Add(station);
        }
    }

    public void SpawnTrains(List<TrainData> trains)
    {
        foreach (TrainData trainData in trains)
        {
            TrainScript train = trainScene.Instantiate<TrainScript>();
            train.Initialize(trainData);
            this.trains.Add(train);
        }
    }

    public void SpawnRobots(List<RobotData> robots)
    {
        foreach (RobotData robotData in robots)
        {
            RobotScript robot = robotScene.Instantiate<RobotScript>();
            robot.Initialize(robotData);
            this.robots.Add(robot);
        }
    }

    public void UpdateTrains(List<TrainData> trains)
    {
        foreach (TrainData trainData in trains)
        {
            TrainScript train = this.trains.Find(train => train.Data.TrainId == trainData.TrainId);
            train?.Update(trainData);
        }
    }

    public void UpdateRobots(List<RobotData> robots)
    {
        foreach (RobotData robotData in robots)
        {
            RobotScript robot = this.robots.Find(train => train.Data.TrainId == robotData.RobotId);
            robot?.Update(robotData);
        }
    }

    public void Reset()
    {
        robots.ForEach(robot => robot.QueueFree());
        robots.Clear();
        trains.ForEach(train => train.QueueFree());
        trains.Clear();
        stations.ForEach(station => station.QueueFree());
        stations.Clear();
    }

    public StationScript FindStation(string stationId)
    {
        StationScript station = stations.Find(station => station.Data.StationId == stationId);
        return station;
    }

    public TrainScript FindTrain(int trainId)
    {
        TrainScript train = trains.Find(train => train.Data.TrainId == trainId);
        return train;
    }

    public int GetStationCount()
    {
        return stations.Count;
    }
}
